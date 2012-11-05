using System;
using System.Collections.Generic;

using AppMetrics.Shared;

namespace AppMetrics.AgentService
{
	class AppSettings : AppSettingsBase
	{
		public static AppSettings Load()
		{
			var res = Load<AppSettings>(FileName);
			if (string.IsNullOrWhiteSpace(res.ConfigBaseUrl))
				throw new ApplicationException("ConfigBaseUrl config option is missing");
			return res;
		}

		private static readonly string FileName = Const.WorkingAreaPath + "AppSettings.xml";

		public string ConfigBaseUrl { get; set; }

		public string AutoUpdateUrl
		{
			get { return ConfigBaseUrl + "/CIAPILatencyCollector/updates/"; }
		}
	}
}
