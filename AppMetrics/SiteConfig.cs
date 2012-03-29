using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;

namespace AppMetrics
{
	public static class SiteConfig
	{
		public static string DataStoragePath
		{
			get
			{
				var res = Get("DataStoragePath");
				if (!res.Contains(':')) // not an absolute path
				{
					if (res.StartsWith(".")) // relative path
						res = Path.GetFullPath(HostingEnvironment.MapPath("~") + "\\" + res);
					else
						res = HostingEnvironment.MapPath(res); // resolve as site relative path
				}
				return res;
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

		static string Get(string name)
		{
			var tmp = Config.AppSettings.Settings[name];
			if (tmp == null)
				return null;
			return tmp.Value;
		}

		private static Configuration _config;

		static Configuration Config
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