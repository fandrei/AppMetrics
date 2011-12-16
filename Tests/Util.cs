using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tests
{
	static class Util
	{
		public static string GetAppLocation()
		{
			var location = Assembly.GetExecutingAssembly().CodeBase;
			location = (new Uri(location)).LocalPath;
			var res = Path.GetDirectoryName(location) + "\\";
			return res;
		}
	}
}
