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

				using (var writer = new StreamWriter(filePath, true, Encoding.UTF8))
				{
					var i = 0;
					while (true)
					{
						var paramName = "TrackerData" + i;
						var logData = context.Request.Params[paramName];
						if (logData == null)
							break;

						writer.WriteLine(logData);

						i++;
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

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}
	}
}