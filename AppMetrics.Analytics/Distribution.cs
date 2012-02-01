using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.Analytics
{
	public class Distribution
	{
		public SortedDictionary<decimal, int> Vals = new SortedDictionary<decimal, int>();
		public int Count;
	}
}
