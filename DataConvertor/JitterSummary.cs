using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.DataConvertor
{
	class JitterSummary
	{
		public int Count;
		public SortedDictionary<decimal, decimal> Vals = new SortedDictionary<decimal, decimal>();
	}
}
