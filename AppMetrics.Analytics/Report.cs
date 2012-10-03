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
			return GetLatencyStatSummariesReport(results, calc => calc.StatSummary);
		}

		public static string GetStreamingLatencyStatSummariesReport(IEnumerable<CalcResult> results)
		{
			return GetLatencyStatSummariesReport(results, calc => calc.StreamingStatSummary);
		}

		public static string GetLatencyStatSummariesReport(IEnumerable<CalcResult> results, Func<CalcResult, StatSummary> selector)
		{
			var res = new StringBuilder(DefaultBufferSize);

			res.Append("Country\tCity\tLocation\tFunctionName");
			res.AppendLine("\tCount\tExceptionsCount\tAverage\tMin\tPercentile2\tLowerQuartile\tMedian\tUpperQuartile\tPercentile98\tMax");

			foreach (var result in results)
			{
				var summary = selector(result);
				if (summary == null)
					continue;
				res.AppendFormat("{0}\t{1}\t{2}\t{3}", result.Country, result.City, result.Location, result.FunctionName);
				res.AppendLine("\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
					summary.Count, result.ExceptionsCount, summary.Average,
					summary.Min, summary.Percentile2, summary.LowerQuartile, summary.Median,
					summary.UpperQuartile, summary.Percentile98, summary.Max);
			}

			return res.ToString();
		}

		public static string GetLatencyDistributionReport(IEnumerable<CalcResult> results)
		{
			return GetDistributionReport(results, "Latency", calc => calc.LatencyDistribution);
		}

		public static string GetJitterDistributionReport(IEnumerable<CalcResult> results)
		{
			return GetDistributionReport(results, "Difference", calc => calc.StreamingJitter);
		}

		public static string GetStreamingLatencyDistributionReport(IEnumerable<CalcResult> results)
		{
			return GetDistributionReport(results, "Latency", calc => calc.StreamingLatencyDistribution);
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

		public static string GetExceptionsReport(IEnumerable<CalcResult> results)
		{
			var res = new StringBuilder(DefaultBufferSize);

			foreach (var result in results)
			{
				res.AppendLine("{0}\r\n{1}\r\n{2}",
					Delimiter1, result.Location, Delimiter1);

				foreach (var pair in result.Exceptions)
				{
					res.AppendLine("{0}\r\n{1}\r\n{2}\r\n{3}",
						pair.Key, Delimiter2, pair.Value, Delimiter2);
				}
			}

			return res.ToString();
		}

		private static readonly string Delimiter1 = new string('=', 40);
		private static readonly string Delimiter2 = new string('-', 40);

		public static void AppendLine(this StringBuilder res, string format, params object[] args)
		{
			var tmp = string.Format(format, args);
			res.AppendLine(tmp);
		}

		private const int DefaultBufferSize = 1024 * 1024; // use the same buffer size to prevent LOH fragmentation
	}
}