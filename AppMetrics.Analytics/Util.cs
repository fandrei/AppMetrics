using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.Analytics
{
	class Util
	{
		public static decimal Ceiling(decimal val, decimal period)
		{
			var count = 1 / period;
			var res = Math.Ceiling(val * count) / count;
			return res;
		}

		public static bool IsLatency(RecordEx record)
		{
			return record.Name.StartsWith("Latency");
		}

		public static bool IsJitter(RecordEx record)
		{
			return record.Name.StartsWith("Jitter");
		}

		public static SortedDictionary<TKey, List<TSource>> GroupBySorted<TSource, TKey>(IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector)
		{
			var res = source.GroupBy(keySelector).ToDictionary(pair => pair.Key, pair => pair.ToList());
			return new SortedDictionary<TKey, List<TSource>>(res);
		}

		public static Dictionary<TKey, List<TSource>> GroupBy<TSource, TKey>(IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector)
		{
			var res = source.GroupBy(keySelector).ToDictionary(pair => pair.Key, pair => pair.ToList());
			return new Dictionary<TKey, List<TSource>>(res);
		}
	}
}
