﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.Analytics
{
	public class StatSummary
	{
		public decimal Average;

		public decimal Median;
		public decimal LowerQuartile;
		public decimal UpperQuartile;
		public decimal Min;
		public decimal Max;
		public decimal Percentile2;
		public decimal Percentile98;

		public int Count;
	}
}
