using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AppMetrics.WebUtils
{
	class WebLogger
	{
		public static void Report(object val)
		{
			var msg = val.ToString() + Environment.NewLine;
			Trace.WriteLine(msg);

			try
			{
				var filePath = Path.Combine(WebUtil.AppDataPath, "log.txt");
				File.AppendAllText(filePath, msg);
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
			}
		}
	}
}
