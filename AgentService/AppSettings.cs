using System;
using System.Collections.Generic;

using AppMetrics.Shared;

namespace AppMetrics.AgentService
{
	class AppSettings : AppSettingsBase
	{
		public static AppSettings Load()
		{
			return Load<AppSettings>(FileName);
		}

		private static readonly string FileName = Const.WorkingAreaPath + "AppSettings.xml";

		public string ConfigBaseUrl { get; set; }

		public string AutoUpdateUrl
		{
			get { return ConfigBaseUrl + "/CIAPILatencyCollector/updates/"; }
		}
	}
}
