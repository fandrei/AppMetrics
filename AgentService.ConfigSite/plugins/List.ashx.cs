using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

using AppMetrics.WebUtils;

namespace AppMetrics.AgentService.ConfigSite.plugins
{
	public class List : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "text/plain";
			try
			{
				var buf = new StringBuilder();
				var pluginsPath = WebUtil.GetWebAppPath() + @"\plugins";

				foreach (var dir in Directory.GetDirectories(pluginsPath))
				{
					var versionFilePath = dir + @"\version.txt";
					var version = File.ReadAllText(versionFilePath);
					var pluginName = Path.GetFileName(dir);
					buf.AppendFormat("{0} {1}\r\n", pluginName, version);
				}

				context.Response.Write(buf.ToString());
			}
			catch (Exception exc)
			{
				context.Response.Write(exc.ToString());
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