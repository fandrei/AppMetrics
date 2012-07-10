using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppMetrics.WebUtils;

namespace AppMetrics
{
	/// <summary>
	/// Summary description for GetSessions
	/// </summary>
	public class GetSessions : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			try
			{
				var requestParams = context.Request.Params;

				var appKey = requestParams.Get("Application") ?? "";

				var period = new TimePeriod(requestParams);

				var sessions = DataReader.GetSessions(appKey, period);

				WebUtil.TryEnableCompression(context);
				context.Response.ContentType = "text/plain";
				foreach (var session in sessions)
				{
					context.Response.Write(session.Serialize());
					context.Response.Write(Environment.NewLine);
				}
			}
			catch (Exception exc)
			{
				WebLogger.Report(exc);
				throw;
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