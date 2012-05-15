using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppMetrics.DataModel;

namespace AppMetrics
{
	/// <summary>
	/// Summary description for GetRecords
	/// </summary>
	public class GetRecords : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			var requestParams = context.Request.Params;

			var appKey = requestParams.Get("AppKey") ?? "";

			var sessionId = requestParams.Get("AppKey") ?? "";

			var startTimeString = requestParams.Get("StartTime") ?? "";
			var startTime = string.IsNullOrEmpty(startTimeString) ? DateTime.MinValue : DateTime.Parse(startTimeString);

			context.Response.ContentType = "text/plain";

			List<Record> records;
			if (string.IsNullOrEmpty(sessionId))
				records = DataSource.GetRecords(appKey, startTime);
			else
			{
				var session = DataSource.ReadSession(appKey, sessionId, startTime);
				records = DataSource.GetRecordsFromSession(session, startTime, true);
			}

			foreach (var record in records)
			{
				var text = record.Serialize();
				context.Response.Write(text);
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