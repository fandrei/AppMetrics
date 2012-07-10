using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AppMetrics.Shared;
using AppMetrics.WebUtils;

namespace AppMetrics
{
	/// <summary>
	/// Summary description for GetRecords
	/// </summary>
	public class GetRecords : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			try
			{
				var requestParams = context.Request.Params;

				var appKey = requestParams.Get("Application") ?? "";
				var sessionId = requestParams.Get("SessionId") ?? "";
				var period = new TimePeriod(requestParams);

				WebUtil.TryEnableCompression(context);
				context.Response.ContentType = "text/plain";

				List<Record> records;
				if (string.IsNullOrEmpty(sessionId))
					records = DataReader.GetRecords(appKey, period);
				else
				{
					var session = DataReader.ReadSession(appKey, sessionId, period);
					if (session == null)
						return;
					records = DataReader.GetRecordsFromSession(session, period, true);
				}

				foreach (var record in records)
				{
					var text = record.Serialize();
					context.Response.Write(text);
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