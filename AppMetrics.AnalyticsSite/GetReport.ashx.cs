using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace AppMetrics.Analytics
{
	/// <summary>
	/// Creates reports and delivers to the user
	/// </summary>
	public class GetReport : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "text/plain";
			lock (Sync)
			{
				if (string.IsNullOrEmpty(_reportText) || DateTime.UtcNow - _lastUpdate > CacheDuration)
				{
					_reportText = CreateReport();
					_lastUpdate = DateTime.UtcNow;
				}
				string status = string.Format("Generated at: {0}\tPeriod: {1}\r\n", _lastUpdate, ReportPeriod);
				context.Response.Write(status);
				context.Response.Write(_reportText);
			}
		}

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}

		static string CreateReport()
		{
			var watch = Stopwatch.StartNew();

			var dataPath = AppSettings.DataStoragePath + @"\CIAPI.CS.Excel";
			var sessions = LogReader.Parse(dataPath, ReportPeriod);

			var convertor = new StatsBuilder();
			var options = new AnalysisOptions { SliceByLocation = false, SliceByFunction = false };
			var res = convertor.Process(sessions, options);

			var latencyReport = Report.GetLatencyStatSummariesReport(res);

			watch.Stop();
			Trace.WriteLine(watch.Elapsed.TotalSeconds);

			return latencyReport;
		}

		private static readonly object Sync = new object();
		private static string _reportText = "";
		private static DateTime _lastUpdate;
		private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(10);
		private static readonly TimeSpan ReportPeriod = TimeSpan.FromMinutes(1);
	}
}