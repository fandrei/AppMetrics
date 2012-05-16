using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AppMetrics.Shared;

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
			var startTime = string.IsNullOrEmpty(startTimeString) ? DateTime.MinValue : Util.ParseDateTime(startTimeString);

			context.Response.ContentType = "text/plain";

			List<Record> records;
			if (string.IsNullOrEmpty(sessionId))
				records = DataReader.GetRecords(appKey, startTime);
			else
			{
				var session = DataReader.ReadSession(appKey, sessionId, startTime);
				records = DataReader.GetRecordsFromSession(session, startTime, true);
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