using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.DataConvertor
{
	static class Stats
	{
		public static void FindQuartiles(IEnumerable<decimal> vals,
			out decimal median, out decimal lowerQuartile, out decimal upperQuartile)
		{
			var sorted = new List<decimal>(vals);
			sorted.Sort();

			var medianIndex = GetMedianIndex(sorted.Count) - 1;
			median = FindValue(sorted, medianIndex);

			var lowerQuartileIndex = GetMedianIndex(Math.Floor(medianIndex) + 1) - 1;
			lowerQuartile = FindValue(sorted, lowerQuartileIndex);

			var upperQuartileIndex = Math.Ceiling(medianIndex) + GetMedianIndex(sorted.Count - Math.Ceiling(medianIndex)) - 1;
			upperQuartile = FindValue(sorted, upperQuartileIndex);
		}

		private static decimal GetMedianIndex(decimal count)
		{
			return (count + 1) / 2;
		}

		static decimal FindValue(IList<decimal> vals, decimal i)
		{
			var rounded = Math.Floor(i);
			if (i == rounded)
				return vals[(int)rounded];
			else
				return (vals[(int)rounded] + vals[(int)rounded + 1]) / 2;
		}
	}
}
