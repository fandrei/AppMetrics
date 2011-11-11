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
				var sessionId = context.Request.Params["TrackerSession"];
				var dataRootPath = Path.GetFullPath(context.Request.PhysicalApplicationPath);
				var time = DateTime.UtcNow.ToString("u");
				time = time.Replace(':', '_');
				var filePath = Path.GetFullPath(string.Format("{0}\\Data\\{1}.{2}.txt", dataRootPath, time, sessionId));
				if (!filePath.StartsWith(dataRootPath)) // block malicious session ids
					throw new ArgumentException(filePath);

				var logData = context.Request.Params["TrackerData"];

				using (var writer = new StreamWriter(filePath, true, Encoding.Unicode))
				{
					writer.WriteLine(logData);
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