using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AppMetrics.AgentService.ConfigSite
{
	/// <summary>
	/// Summary description for GetConfig
	/// </summary>
	public class GetConfig : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "text/plain";

			var configText = "";

			var nodeName = context.Request.Params.Get("NodeName");
			var pluginName = context.Request.Params.Get("PluginName");
			var nodeStatus = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

			if (File.Exists(Const.StopFileName))
			{
				configText = "disabled";
				nodeStatus = nodeStatus + " (" + configText + ")";
			}
			else
			{
				var configRoot = Const.ConfigBasePath + "/" + pluginName + "/config/";
				var configPath = configRoot + nodeName + "/" + Const.NodeSettingsFileName;
				if (File.Exists(configPath))
				{
					configText = File.ReadAllText(configPath);
				}
				else
				{
					var defaultConfigPath = configRoot + Const.NodeSettingsFileName;
					if (File.Exists(defaultConfigPath))
						configText = File.ReadAllText(defaultConfigPath);
				}
			}

			context.Response.Write(configText);

			NodeNames[nodeName] =  nodeStatus;
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

		public static readonly SortedList<string, string> NodeNames = new SortedList<string, string>();
	}
}