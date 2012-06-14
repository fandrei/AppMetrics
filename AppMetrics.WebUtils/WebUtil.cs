using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace AppMetrics.WebUtils
{
	public static class WebUtil
	{
		public static void CheckIpAddress()
		{
			var request = HttpContext.Current.Request;
			if (!request.IsLocal)
			{
				var response = HttpContext.Current.Response;
				response.Write("Access from this IP address is not allowed");
				response.Status = "401 Unauthorized";
				response.StatusCode = 401;
				response.End();
				throw new UnauthorizedAccessException();
			}
		}

		public static string AppDataPath
		{
			get
			{
				var res = HostingEnvironment.MapPath("~/App_Data");
				return res;
			}
		}
	}
}