using System;
using System.Collections.Generic;
using System.Diagnostics;
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

			var requestParams = context.Request.QueryString;
			var application = requestParams.Get("Application");
			if (application == null)
			{
				context.Response.Write("Application key is not defined");
				return;
			}
			var countries = requestParams.Get("Locations") ?? "";
			var countryList = countries.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

			var options = new AnalysisOptions
			{
				ApplicationKey = application,
				LocationIncludeOverall = false,
				SliceByLocation = (countryList.Length > 0) ? LocationSliceType.Countries : LocationSliceType.None,
				SliceByFunction = false,
				CountryFilter = new HashSet<string>(countryList),
				Period = ReportPeriod,
			};

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

			var status = string.Format("Period: {0}\tGenerated at: {1}\tGeneration time: {2}\r\n",
				options.Period, report.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"), report.GenerationElapsed);
			context.Response.Write(status);
			context.Response.Write(report.ReportText);
		}

		private static void RemoveOutdatedReports()
		{
			lock (Sync)
			{
				var now = DateTime.UtcNow;
				var forRemoval = CachedReports.Where(
					pair =>
					{
						var options = pair.Key;
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

		private static readonly object Sync = new object();
		private static readonly Dictionary<AnalysisOptions, ReportInfo> CachedReports =
			new Dictionary<AnalysisOptions, ReportInfo>();
		private static readonly TimeSpan ReportPeriod = TimeSpan.FromMinutes(1);
		private static readonly TimeSpan CacheInvalidationPeriod = TimeSpan.FromSeconds(60);
	}
}