using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.Analytics
{
	public class CalcResult
	{
		public StatSummary StatSummary;
		public StatSummary StreamingStatSummary;

		public Distribution LatencyDistribution;
		public Distribution StreamingLatencyDistribution;

		public Distribution StreamingJitter;

		public int ExceptionsCount;

		public Dictionary<string, int> Exceptions = new Dictionary<string, int>();

		public string FunctionName;
		public string City;
		public string Country;
		public string Region;

		public string Location
		{
			get
			{
				if (string.IsNullOrEmpty(City))
					return Country;
				return string.Format("{0}/{1}/{2}", Country, Region, City);
			}
		}

		public override string ToString()
		{
			var res = string.Format("'{0}' '{1}' '{2}'", Country, City, FunctionName);
			return res;
		}
	}
}
