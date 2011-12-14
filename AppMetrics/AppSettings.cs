using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace AppMetrics
{
	public static class AppSettings
	{
		public static string DataStoragePath
		{
			get
			{
				var res = Config.AppSettings.Settings["DataStoragePath"].Value;
				if (!res.Contains(':')) // not an absolute path
					res = HttpContext.Current.Server.MapPath(res); // resolve as site relative path
				return res;
			}
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