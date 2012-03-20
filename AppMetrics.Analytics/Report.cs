using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.Analytics
{
	public static class Report
	{
		public static string GetSummaryReport(ICollection<SessionEx> sessions)
		{
			var res = new StringBuilder(DefaultBufferSize);

			res.AppendLine("Name\tValue");

			if (sessions.Count > 0)
			{
				var minDate = sessions.Min(session => session.LastUpdateTime);
				res.AppendLine("MinDate\t{0}", minDate.ToString("yyyy-MM-dd HH:mm:ss"));

				var maxDate = sessions.Max(session => session.LastUpdateTime);
				res.AppendLine("MaxDate\t{0}", maxDate.ToString("yyyy-MM-dd HH:mm:ss"));
			}

			// append leading space as a workaround for the PowerPivot quirk 
			// http://social.msdn.microsoft.com/Forums/en-US/sqlkjpowerpivotforexcel/thread/456699ec-b5a2-4ae9-bc9f-b7ed2d637959
			res.AppendLine("SessionsCount\t {0}", sessions.Count);

			var latencyRecordsCount = sessions.Aggregate(0,
				(val, session) => val + session.Records.Where(Util.IsLatency).Count());
			res.AppendLine("LatencyRecordsCount\t {0}", latencyRecordsCount);

			var jitterRecordsCount = sessions.Aggregate(0,
				(val, session) => val + session.Records.Where(Util.IsJitter).Count());
			res.AppendLine("JitterRecordsCount\t {0}", jitterRecordsCount);

			return res.ToString();
		}

		public static string GetLatencyStatSummariesReport(IEnumerable<CalcResult> results)
		{
			var res = new StringBuilder(DefaultBufferSize);

			res.AppendLine("Country\tCity\tLocation\tFunctionName\tCount\tAverage\tMin\tLowerQuartile\tMedian\tUpperQuartile\tMax");

			foreach (var result in results)
			{
				var summary = result.StatSummary;
				if (summary == null)
					continue;
				res.AppendLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}",
					result.Country, result.City, result.Location, result.FunctionName,
					summary.Count, summary.Average,
					summary.Min, summary.LowerQuartile, summary.Median, summary.UpperQuartile, summary.Max);
			}

			return res.ToString();
		}

		public static string GetLatencyDistributionReport(IEnumerable<CalcResult> results)
		{
			return GetDistributionReport(results, "Latency", calc => calc.Distribution);
		}

		public static string GetJitterDistributionReport(IEnumerable<CalcResult> results)
		{
			return GetDistributionReport(results, "Difference", calc => calc.Jitter);
		}

		public static string GetDistributionReport(IEnumerable<CalcResult> results, string paramName,
			Func<CalcResult, Distribution> selector)
		{
			var res = new StringBuilder(DefaultBufferSize);

			res.AppendLine("Country\tCity\tLocation\tFunctionName\t{0}\tCount", paramName);

			foreach (var result in results)
			{
				var cur = selector(result);
				if (cur == null)
					continue;
				foreach (var pair in cur.Vals)
				{
					res.AppendLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
					result.Country, result.City, result.Location, result.FunctionName,
					pair.Key, pair.Value);
				}
			}

			return res.ToString();
		}

		public static string GetAveragePercentile98Report(IEnumerable<CalcResult> results)
		{
			var res = new StringBuilder(DefaultBufferSize);

			res.AppendLine("Country\tCity\tLocation\tFunctionName\tAveragePercentile98\tTotalCount\tOutliersCount");

			foreach (var result in results)
			{
				var cur = result.Percentile98;
				if (cur == null)
					continue;

				res.AppendLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
					result.Country, result.City, result.Location, result.FunctionName,
					cur.Average, cur.TotalCount, cur.OutliersCount);
			}

			return res.ToString();
		}

		public static void AppendLine(this StringBuilder res, string format, params object[] args)
		{
			var tmp = string.Format(format, args);
			res.AppendLine(tmp);
		}

		private const int DefaultBufferSize = 1024 * 1024; // use the same buffer size to prevent LOH fragmentation
	}
}