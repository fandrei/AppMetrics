using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;

namespace AppMetrics.AgentService.ConfigSite
{
	public static class Const
	{
		public static string ConfigBasePath
		{
			get
			{
				const string tmp = "~/plugins/";
				return HostingEnvironment.MapPath(tmp);
			}
		}

		public static string StopFileName
		{
			get
			{
				return ConfigBasePath + "stop.txt";
			}
		}

		public const string NodeSettingsFileName = "AppSettings.xml";
	}
}