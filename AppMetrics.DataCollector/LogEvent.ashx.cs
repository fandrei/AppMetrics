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
				var sessionId = context.Request.Params["MessageSession"];
				if (string.IsNullOrEmpty(sessionId))
					throw new ApplicationException("No session ID");

				var dataRootPath = Path.GetFullPath(context.Request.PhysicalApplicationPath);
				var time = DateTime.UtcNow.ToString("u");
				time = time.Replace(':', '_');
				var filePath = Path.GetFullPath(string.Format("{0}\\Data\\{1}.{2}.txt", dataRootPath, time, sessionId));
				if (!filePath.StartsWith(dataRootPath)) // block malicious session ids
					throw new ArgumentException(filePath);

				using (var writer = new StreamWriter(filePath, true, Encoding.UTF8))
				{
					var name = context.Request.Params["MessageName"];
					var data = context.Request.Params["MessageData"];
					var clientTime = context.Request.Params["MessageTime"];

					writer.Write("{0}\t{1}", clientTime, name);
					if (!data.Contains('\n'))
						writer.WriteLine("\t{0}", data);
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