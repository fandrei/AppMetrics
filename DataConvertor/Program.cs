using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using AppMetrics.Analytics;
using AppMetrics.Shared;

namespace AppMetrics.DataConvertor
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length == 0 || args.Length > 3)
					throw new ApplicationException("Invalid command line args");

				var listeners = new[] { new TextWriterTraceListener(Console.Out) };
				Debug.Listeners.AddRange(listeners);

				var dataPath = args[0];
				var resPath = (args.Length >= 2) ? args[1] : ".";
				resPath = Path.GetFullPath(resPath);
				var period = (args.Length >= 3) ? TimeSpan.Parse(args[2]) : TimeSpan.MaxValue;

				ProcessData(resPath, dataPath, period);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}

		private static void ProcessData(string resPath, string dataPath, TimeSpan period)
		{
			var sessions = LogReader.Parse(dataPath, TimePeriod.Create(period));
			sessions.AddRange(IisLogReader.ReadIisData(dataPath));

			var options = new AnalysisOptions
				{
					LocationIncludeOverall = true
				};

			var convertor = new StatsBuilder();
			var res = convertor.Process(sessions, options);

			var summaryReport = Report.GetSummaryReport(sessions);
			File.WriteAllText(resPath + "\\Summary.txt", summaryReport, Encoding.UTF8);

			var statSummariesReport = Report.GetLatencyStatSummariesReport(res);
			File.WriteAllText(resPath + "\\LatencyStatSummaries.txt", statSummariesReport, Encoding.UTF8);

			var latencyDistributionReport = Report.GetLatencyDistributionReport(res);
			File.WriteAllText(resPath + "\\LatencyDistribution.txt", latencyDistributionReport, Encoding.UTF8);

			var jitterDsitributionReport = Report.GetJitterDistributionReport(res);
			File.WriteAllText(resPath + "\\JitterDistribution.txt", jitterDsitributionReport, Encoding.UTF8);
		}
	}
}
