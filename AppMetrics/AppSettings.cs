using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

using AppMetrics.Shared;
using AppMetrics.WebUtils;

namespace AppMetrics
{
	public class AppSettings : AppSettingsBase
	{
		public bool RequireAccessKey { get; set; }
		public string[] AccessKeys { get; set; }

		private static AppSettings _instance;
		static readonly object Sync = new object();

		public static AppSettings Instance
		{
			get
			{
				lock (Sync)
				{
					if (_instance == null)
						Load(SiteConfig.DataStoragePath + @"\settings.xml");
					return _instance;
				}
			}
		}

		public static void Load(string fileName)
		{
			lock (Sync)
			{
				_instance = Load<AppSettings>(fileName);
			}
		}
	}
}