using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppMetrics
{
	public static class WebUtil
	{
		public static void CheckIpAddress()
		{
			var request = HttpContext.Current.Request;
			var ip = request.UserHostAddress;
			if (ip != "127.0.0.1" && ip != "::1")
			{
				var response = HttpContext.Current.Response;
				response.Write("Access from this IP address is not allowed");
				response.Status = "401 Unauthorized";
				response.StatusCode = 401;
				response.End();
				throw new UnauthorizedAccessException();
			}
		}
	}
}