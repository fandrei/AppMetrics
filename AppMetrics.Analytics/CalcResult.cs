﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.Analytics
{
	class CalcResult
	{
		public StatSummary StatSummary;
		public Distribution Distribution;
		public JitterSummary Jitter;

		public string FunctionName;
		public string City;
		public string Country;
		public string Location
		{
			get
			{
				if (string.IsNullOrEmpty(City))
					return Country;
				return string.Format("{0}/{1}", Country, City);
			}
		}

		public override string ToString()
		{
			var res = string.Format("'{0}' '{1}' '{2}'", Country, City, FunctionName);
			return res;
		}
	}
}