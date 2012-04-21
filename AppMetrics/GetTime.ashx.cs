using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppMetrics
{
	/// <summary>
	/// Summary description for GetTime
	/// </summary>
	public class GetTime : IHttpHandler
	{

		public void ProcessRequest(HttpContext context)
		{
			var time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");

			context.Response.ContentType = "text/plain";
			context.Response.Write(time);
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