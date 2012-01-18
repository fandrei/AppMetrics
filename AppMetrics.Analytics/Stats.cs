using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.Analytics
{
	static class Stats
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
	}
}
