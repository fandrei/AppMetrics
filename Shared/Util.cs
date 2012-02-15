using System;
using System.Collections.Generic;
using System.Linq;

namespace AppMetrics
{
	public static class Util
	{
		public static string Escape(string val)
		{
			var res = val.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
			return res;
		}
	}
}