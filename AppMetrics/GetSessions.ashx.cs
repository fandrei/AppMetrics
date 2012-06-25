using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppMetrics
{
	/// <summary>
	/// Summary description for GetSessions
	/// </summary>
	public class GetSessions : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			var requestParams = context.Request.Params;

			var appKey = requestParams.Get("Application") ?? "";

			var period = new TimePeriod(requestParams);

			var sessions = DataReader.GetSessions(appKey, period);

			context.Response.ContentType = "text/plain";
			foreach (var session in sessions)
			{
				context.Response.Write(session.Serialize());
				context.Response.Write(Environment.NewLine);
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