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
				if (string.IsNullOrEmpty(_reportText) || DateTime.UtcNow - _lastUpdate > UpdatePeriod)
				{
					CreateReport();
					_lastUpdate = DateTime.UtcNow;
				}
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

		static void CreateReport()
		{
			var watch = Stopwatch.StartNew();

			var dataPath = AppSettings.DataStoragePath + @"\CIAPI.CS.Excel";
			var period = TimeSpan.FromHours(1);
			var sessions = LogReader.Parse(dataPath, period);

			var convertor = new StatsBuilder();
			var options = new AnalysisOptions { SliceByLocation = false, SliceByFunction = false };
			var res = convertor.Process(sessions, options);

			var latencyReport = Report.GetLatencyStatSummariesReport(res);
			lock (Sync)
			{
				_reportText = latencyReport;
			}

			watch.Stop();
			Trace.WriteLine(watch.Elapsed.TotalSeconds);
		}

		private static readonly object Sync = new object();
		private static string _reportText = "";
		private static DateTime _lastUpdate;
		private static readonly TimeSpan UpdatePeriod = TimeSpan.FromSeconds(10);
	}
}