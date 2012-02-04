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
				if (string.IsNullOrEmpty(_reportText) || DateTime.UtcNow - _lastUpdateTime > CacheDuration)
				{
					var watch = Stopwatch.StartNew();
					_reportText = CreateReport();
					watch.Stop();
					_generationElapsed = watch.Elapsed;
					_lastUpdateTime = DateTime.UtcNow;
				}
				var status = string.Format("Period: {0}\tGenerated at: {1}\tGeneration elapsed time: {2}\r\n",
					ReportPeriod, _lastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"), _generationElapsed);
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
			var dataPath = AppSettings.DataStoragePath + @"\CIAPI.CS.Excel";
			var sessions = LogReader.Parse(dataPath, ReportPeriod);

			var convertor = new StatsBuilder();
			var options = new AnalysisOptions { SliceByLocation = false, SliceByFunction = false };
			var res = convertor.Process(sessions, options);

			var latencyReport = Report.GetLatencyStatSummariesReport(res);

			return latencyReport;
		}

		private static readonly object Sync = new object();
		private static string _reportText = "";
		private static DateTime _lastUpdateTime;
		private static TimeSpan _generationElapsed;
		private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(10);
		private static readonly TimeSpan ReportPeriod = TimeSpan.FromMinutes(1);
	}
}