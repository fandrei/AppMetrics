﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.DataConvertor
{
	class Util
	{
		public static decimal Ceiling(decimal val, decimal period)
		{
			var count = 1 / period;
			var res = Math.Ceiling(val * count) / count;
			return res;
		}

		public static decimal Ceiling(decimal val, double period)
		{
			return Ceiling(val, (decimal)period);
		}

		public static decimal Ceiling(decimal val, int period)
		{
			return Ceiling(val, (decimal)period);
		}
	}
}
