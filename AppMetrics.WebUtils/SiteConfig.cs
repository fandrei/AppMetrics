using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;

namespace AppMetrics.WebUtils
{
	public static class SiteConfig
	{
		public static string DataStoragePath
		{
			get
			{
				var res = Get("DataStoragePath");
				return WebUtil.ResolvePath(res);
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

		public static string Get(string name)
		{
			var tmp = Config.AppSettings.Settings[name];
			if (tmp == null)
				return null;
			return tmp.Value;
		}

		private static Configuration _config;

		public static Configuration Config
		{
			get
			{
				if (_config == null)
					_config = WebConfigurationManager.OpenWebConfiguration("~/App_Data");
				return _config;
			}
		}
	}
}