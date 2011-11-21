using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AppMetrics
{
	public static class Util
	{
		public static string GetDataFolderPath()
		{
			var root = HttpContext.Current.Request.PhysicalApplicationPath;
			var res = Path.Combine(root, "App_Data");
			return res;
		}
	}
}