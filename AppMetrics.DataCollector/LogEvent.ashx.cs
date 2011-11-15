﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace AppMetrics.DataCollector
{
	/// <summary>
	/// Summary description for LogEvent
	/// </summary>
	public class LogEvent : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			try
			{
				var sessionId = context.Request.Params["MessageSession"];
				if (string.IsNullOrEmpty(sessionId))
					throw new ApplicationException("No session ID");

				var filePath = GetDataFilePath(context, sessionId);

				using (var writer = new StreamWriter(filePath, true, Encoding.UTF8))
				{
					var name = context.Request.Params["MessageName"];
					var data = context.Request.Params["MessageData"];
					var clientTime = context.Request.Params["MessageTime"];

					bool multiLineData = data.Contains('\n');
					if (multiLineData)
					{
						writer.WriteLine("{0}\t{1}", clientTime, name);
						writer.WriteLine(_delimiter);
						writer.WriteLine(data);
						writer.WriteLine(_delimiter);
					}
					else
					{
						writer.WriteLine("{0}\t{1}\t{2}", clientTime, name, data);
					}
				}
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
#if DEBUG
				context.Response.Write(exc);
#endif
			}
		}

		private static string GetDataFilePath(HttpContext context, string sessionId)
		{
			var dataRootPath = Path.GetFullPath(context.Request.PhysicalApplicationPath + "\\Data");
			var filesMask = string.Format("*.{0}.txt", sessionId);
			var files = Directory.GetFiles(dataRootPath, filesMask);
			if (files.Length > 0)
			{
				if (files.Length != 1)
					throw new ApplicationException(string.Format("Too much data files for session {0}", sessionId));
				return files[0];
			}
			else
			{
				var time = DateTime.UtcNow.ToString("u");
				time = time.Replace(':', '_');
				var filePath = Path.GetFullPath(string.Format("{0}\\{1}.{2}.txt", dataRootPath, time, sessionId));
				if (!filePath.StartsWith(dataRootPath)) // block malicious session ids
					throw new ArgumentException(filePath);
				return filePath;
			}
		}

		private readonly string _delimiter = new string('-', 80);

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}
	}
}