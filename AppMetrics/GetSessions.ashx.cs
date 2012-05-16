using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AppMetrics.Shared;

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

			var appKey = requestParams.Get("AppKey") ?? "";

			var startTimeString = requestParams.Get("StartTime");
			var startTime = string.IsNullOrEmpty(startTimeString) ? DateTime.MinValue : Util.ParseDateTime(startTimeString);

			var periodString = requestParams.Get("Period");
			if (!string.IsNullOrEmpty(periodString))
			{
				var period = TimeSpan.Parse(periodString);
				startTime = DateTime.UtcNow - period;
			}

			var sessions = DataReader.GetSessions(appKey, startTime);

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