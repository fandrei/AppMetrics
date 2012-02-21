using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

using AppMetrics.Analytics;

namespace AppMetrics.AnalyticsSite
{
	/// <summary>
	/// Creates reports and delivers to the user
	/// </summary>
	public class GetReport : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			InitLog();
			ReportLog(string.Format("Request {0}", context.Request.Url.Query));

			var options = GetOptions(context);
			var report = GetOrCreateReport(options);

			var status = string.Format("Period: {0}\tGenerated at: {1}\tGeneration time: {2}\r\n",
				options.Period, report.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"), report.GenerationElapsed);
			context.Response.ContentType = "text/plain";
			context.Response.Write(status);
			context.Response.Write(report.ReportText);
		}

		private static AnalysisOptions GetOptions(HttpContext context)
		{
			var requestParams = context.Request.QueryString;
			var application = requestParams.Get("Application");
			if (application == null)
				throw new ApplicationException("Application key is not defined");
			var countries = requestParams.Get("Locations") ?? "";
			var countryList = countries.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			var includeWorldOverall = (countryList.Length == 0);
			if (countryList.Contains("(World)"))
				includeWorldOverall = true;

			var periodString = requestParams.Get("Period") ?? "";
			var period = string.IsNullOrEmpty(periodString) ? DefaultReportPeriod : TimeSpan.Parse(periodString);

			return new AnalysisOptions
					{
						ApplicationKey = application,
						LocationIncludeOverall = includeWorldOverall,
						SliceByLocation = LocationSliceType.Countries,
						SliceByFunction = false,
						CountryFilter = new HashSet<string>(countryList),
						Period = period,
					};
		}

		private static ReportInfo GetOrCreateReport(AnalysisOptions options)
		{
			ReportInfo report;
			lock (Sync)
			{
				RemoveOutdatedReports();

				if (!CachedReports.TryGetValue(options, out report))
				{
					var watch = Stopwatch.StartNew();
					report = new ReportInfo { ReportText = CreateReport(options) };
					watch.Stop();
					report.GenerationElapsed = watch.Elapsed;
					report.LastUpdateTime = DateTime.UtcNow;

					CachedReports.Add(options, report);
				}
			}
			return report;
		}

		private static void RemoveOutdatedReports()
		{
			lock (Sync)
			{
				var now = DateTime.UtcNow;
				var forRemoval = CachedReports.Where(
					pair =>
					{
						var report = pair.Value;
						var res = now - report.LastUpdateTime >= CacheInvalidationPeriod;
						return res;
					}).ToArray();

				foreach (var pair in forRemoval)
				{
					CachedReports.Remove(pair.Key);
				}
			}
		}

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}

		static string CreateReport(AnalysisOptions options)
		{
			var sessions = LogReader.Parse(options);

			var convertor = new StatsBuilder();
			var res = convertor.Process(sessions, options);

			var latencyReport = Report.GetLatencyStatSummariesReport(res);

			return latencyReport;
		}

		private static StreamWriter _logFile;
		private static readonly object LogSync = new object();

		static void InitLog()
		{
			lock (LogSync)
			{
				if (_logFile == null)
				{
					var logPath = Path.Combine(AppSettings.AppDataPath, Const.LogFileName);
					_logFile = new StreamWriter(logPath, true, Encoding.UTF8) { AutoFlush = true };
				}
			}
		}

		static void ReportLog(string text)
		{
			try
			{
				lock (LogSync)
				{
					if (_logFile != null)
					{
						var time = DateTime.UtcNow;
						bool multiLineData = text.Contains('\n');
						if (multiLineData)
						{
							_logFile.WriteLine(time);
							_logFile.WriteLine(text);
							_logFile.WriteLine(Const.Delimiter);
						}
						else
						{
							_logFile.WriteLine("{0}\t{1}", time, text);
						}
					}
				}
			}
			catch (Exception exc)
			{
				Trace.WriteLine(text);
				Trace.WriteLine(exc);
			}
		}

		private static readonly object Sync = new object();
		private static readonly Dictionary<AnalysisOptions, ReportInfo> CachedReports =
			new Dictionary<AnalysisOptions, ReportInfo>();
		private static readonly TimeSpan DefaultReportPeriod = TimeSpan.FromMinutes(1);
		private static readonly TimeSpan CacheInvalidationPeriod = TimeSpan.FromSeconds(10);
	}
}