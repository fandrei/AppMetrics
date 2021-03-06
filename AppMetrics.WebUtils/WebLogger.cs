﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace AppMetrics.WebUtils
{
	public static class WebLogger
	{
		public static void Report(object val)
		{
			try
			{
				var text = val.ToString();

				var address = "";
				try
				{
					address = HttpContext.Current.Request.UserHostAddress;
				}
				catch (Exception exc)
				{
					Trace.WriteLine(exc);
				}

				text = address + " > " + text;
				Trace.WriteLine(text);

				var filePath = Path.Combine(WebUtil.AppDataPath, FileName);

				var time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
				bool multiLineData = text.Contains('\n');
				var buf = (multiLineData)
					? string.Format("{0} {1}\r\n{2}\r\n{3}\r\n", time, DelimiterShort, text, Delimiter)
					: string.Format("{0}\t{1}\r\n", time, text);

				if (Monitor.TryEnter(Sync, 10 * 1000))
				{
					try
					{
						File.AppendAllText(filePath, buf);
					}
					finally
					{
						Monitor.Exit(Sync);
					}
				}
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
			}
		}

		static readonly string Delimiter = new string('-', 80);
		static readonly string DelimiterShort = new string('-', 80 - 20);
		public const string FileName = "log.txt";
		static readonly object Sync = new object();
	}
}
