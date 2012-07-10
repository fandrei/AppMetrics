using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AppMetrics.WebUtils
{
	public class WebLogger
	{
		public static void Report(object val)
		{
			try
			{
				var text = val.ToString();
				Trace.WriteLine(text);

				var filePath = Path.Combine(WebUtil.AppDataPath, "log.txt");

				var time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
				bool multiLineData = text.Contains('\n');
				var buf = (multiLineData)
					? string.Format("{0}\r\n{1}\r\n{2}\r\n{3}\r\n", time, Delimiter, text, Delimiter)
					: string.Format("{0}\t{1}\r\n", time, text);
				File.AppendAllText(filePath, buf);
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
			}
		}

		static readonly string Delimiter = new string('-', 80);
	}
}
