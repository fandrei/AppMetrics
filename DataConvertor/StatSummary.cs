using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.DataConvertor
{
	class StatSummary
	{
		public decimal Average;

		public decimal Median;
		public decimal LowerQuartile;
		public decimal UpperQuartile;
		public decimal Min;
		public decimal Max;

		public string FunctionName;
		public string City;
		public string Country;

		public override string ToString()
		{
			var res = string.Format("'{0}' '{1}' '{2}'", Country, City, FunctionName);
			return res;
		}
	}
}
