using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
			context.Response.ContentType = "text/plain";
			var queryString = context.Request.Url.Query;
			try
			{
				Init();

				var options = GetOptions(context.Request.QueryString);
				var lookup = GetOrCreateReport(options);
				var report = lookup.Item2;
				ReportLog(lookup.Item1
					? string.Format("request: '{0}' reusing cached", queryString)
					: string.Format("request: '{0}' generated in {1} secs", queryString, report.GenerationElapsed.TotalSeconds));

				var status = string.Format("Period: {0}\tGenerated at: {1}\tGeneration time: {2}\r\n",
					options.Period, report.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"),
					report.GenerationElapsed);
				context.Response.Write(status);

				string reportText;
				switch (options.ReportType)
				{
					case ReportType.LatencySummaries:
						reportText = Report.GetLatencyStatSummariesReport(report.Result);
						break;
					case ReportType.LatencyDistribution:
						reportText = Report.GetLatencyDistributionReport(report.Result);
						break;
					case ReportType.JitterDistribution:
						reportText = Report.GetJitterDistributionReport(report.Result);
						break;
					default:
						throw new NotSupportedException();
				}
				context.Response.Write(reportText);
			}
			catch (ApplicationException exc)
			{
				context.Response.Write(exc.Message);
				var message = string.Format("request: '{0}' error '{1}'", queryString, exc.Message);
				ReportLog(message);
			}
			catch (Exception exc)
			{
				context.Response.Write(exc.ToString());
				var message = string.Format("request: '{0}' exception:\r\n{1}", queryString, exc);
				ReportLog(message);
			}
		}

		private static AnalysisOptions GetOptions(NameValueCollection requestParams)
		{
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

			var reportTypeText = requestParams.Get("Type");
			var reportType = string.IsNullOrEmpty(reportTypeText)
								? ReportType.LatencySummaries
								: (ReportType)Enum.Parse(typeof(ReportType), reportTypeText);

			return new AnalysisOptions
					{
						ApplicationKey = application,
						LocationIncludeOverall = includeWorldOverall,
						SliceByLocation = LocationSliceType.Countries,
						SliceByFunction = false,
						CountryFilter = new HashSet<string>(countryList),
						Period = period,
						ReportType = reportType,
					};
		}

		private static Tuple<bool, ReportInfo> GetOrCreateReport(AnalysisOptions options)
		{
			ReportInfo report;
			bool cached = true;
			lock (Sync)
			{
				RemoveOutdatedReports();

				if (!CachedReports.TryGetValue(options, out report))
				{
					cached = false;
					var watch = Stopwatch.StartNew();
					report = new ReportInfo { Result = CreateReport(options) };
					watch.Stop();
					report.GenerationElapsed = watch.Elapsed;
					report.LastUpdateTime = DateTime.UtcNow;

					CachedReports.Add(options, report);
				}
			}
			return new Tuple<bool, ReportInfo>(cached, report);
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

		static List<CalcResult> CreateReport(AnalysisOptions options)
		{
			var sessions = LogReader.Parse(options);

			var convertor = new StatsBuilder();
			var res = convertor.Process(sessions, options);

			return res;
		}

		static string _logPath;

		static void Init()
		{
			if (_logPath == null)
				_logPath = Path.Combine(SiteConfig.AppDataPath, Const.LogFileName);
		}

		static void ReportLog(string text)
		{
			try
			{
				var time = DateTime.UtcNow;
				var multiLineData = text.Contains('\n');
				var message = multiLineData
					? string.Format("{0}\r\n{1}\r\n{2}\r\n", time, text, Const.Delimiter)
					: string.Format("{0}\t{1}\r\n", time, text);
				File.AppendAllText(_logPath, message, Encoding.UTF8);
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