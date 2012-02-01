using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.Analytics
{
	public static class Stats
	{
		public static StatSummary CalculateSummaries(ICollection<decimal> vals)
		{
			var res = new StatSummary();

			res.Count = vals.Count;
			res.Average = vals.Average();

			var sorted = new List<decimal>(vals);
			sorted.Sort();

			var medianIndex = GetMedianIndex(sorted.Count) - 1;
			res.Median = FindValue(sorted, medianIndex);

			var lowerQuartileIndex = GetMedianIndex(Math.Floor(medianIndex) + 1) - 1;
			res.LowerQuartile = FindValue(sorted, lowerQuartileIndex);

			var upperQuartileIndex = Math.Ceiling(medianIndex) + GetMedianIndex(sorted.Count - Math.Ceiling(medianIndex)) - 1;
			res.UpperQuartile = FindValue(sorted, upperQuartileIndex);

			res.Min = sorted.First();
			res.Max = sorted.Last();

			Validate(sorted, res);
			return res;
		}

		private static decimal GetMedianIndex(decimal count)
		{
			return (count + 1) / 2;
		}

		static decimal FindValue(IList<decimal> vals, decimal i)
		{
			var rounded = (int)Math.Floor(i);
			if (i == rounded)
				return vals[rounded];
			else
				return (vals[rounded] + vals[rounded + 1]) / 2;
		}

		static void Validate(ICollection<decimal> vals, StatSummary summary)
		{
			ValidateSplitterValue(vals, summary.Median, 0.5M);
			ValidateSplitterValue(vals, summary.LowerQuartile, 0.25M);
			ValidateSplitterValue(vals, summary.UpperQuartile, 0.75M);
		}

		private static void ValidateSplitterValue(ICollection<decimal> vals, decimal splitterValue, decimal splittingCoefficient)
		{
			int smallerCount = 0, biggerCount = 0;
			foreach (var val in vals)
			{
				if (val < splitterValue)
					smallerCount++;
				if (val > splitterValue)
					biggerCount++;
			}

			if (smallerCount > Math.Round(vals.Count * splittingCoefficient))
				throw new ValidationException();
			if (biggerCount > Math.Round(vals.Count * (1 - splittingCoefficient)))
				throw new ValidationException();
		}

		public static Distribution CalculateDistribution(decimal[] values, decimal period)
		{
			var res = new Distribution { Count = values.Length };

			foreach (var latency in values)
			{
				var rounded = Util.Ceiling(latency, period);
				if (res.Vals.ContainsKey(rounded))
					res.Vals[rounded]++;
				else
					res.Vals[rounded] = 1;
			}

			return res;
		}
	}
}
