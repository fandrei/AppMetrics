using System;
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
				var sessionId = context.Request.Params["Session"];
				var rootPath = context.Request.PhysicalApplicationPath;
				var time = DateTime.UtcNow.ToString("u");
				time = time.Replace(':', '_');
				var fileName = string.Format("{0}\\Data\\{1}.{2}.txt", rootPath, time, sessionId);

				var logData = context.Request.Params["Data"];

				using (var writer = new StreamWriter(fileName, true, Encoding.Unicode))
				{
					writer.Write(logData);
				}
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
			}
		}

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}
	}
}