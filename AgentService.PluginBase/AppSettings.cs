﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

using AppMetrics.Shared;

namespace AppMetrics.AgentService.PluginBase
{
	public class AppSettings : AppSettingsBase
	{
		public static AppSettings Load()
		{
			var res = Load<AppSettings>(FileName);
			return res;
		}

		protected static readonly string FileName = Const.WorkingAreaPath + "AppSettings.xml";

		public string ConfigBaseUrl { get; set; }

		public string MetricsServerUrl { get; set; }

		public string UserId { get; set; }
		public string NodeName { get; set; }

		public string PluginsUrl
		{
			get { return ConfigBaseUrl + "/plugins/"; }
		}

		public string PluginsListUrl
		{
			get { return ConfigBaseUrl + "/plugins/List.ashx"; }
		}

		public string UserName { get; set; }

		public string PasswordEncrypted { get; set; }
		static readonly byte[] AdditionalEntropy = { 0x43, 0x71, 0xDE, 0x5B, 0x44, 0x72, 0x45, 0xE3, 0xBE, 0x1E, 0x98, 0x2B, 0xAA };

		[XmlIgnore]
		public string Password
		{
			get
			{
				if (string.IsNullOrEmpty(PasswordEncrypted))
					return "";

				var encrypted = Convert.FromBase64String(PasswordEncrypted);
				var data = ProtectedData.Unprotect(encrypted, AdditionalEntropy, DataProtectionScope.LocalMachine);
				var res = Encoding.UTF8.GetString(data);
				return res;
			}
			set
			{
				var data = Encoding.UTF8.GetBytes(value);
				var encrypted = ProtectedData.Protect(data, AdditionalEntropy, DataProtectionScope.LocalMachine);
				PasswordEncrypted = Convert.ToBase64String(encrypted);
			}
		}

		protected override void OnBeforeSave()
		{
			if (string.IsNullOrWhiteSpace(ConfigBaseUrl))
				throw new ApplicationException("ConfigBaseUrl config option is missing");
		}

		protected override void OnAfterLoad()
		{
			if (UserId.IsNullOrEmpty())
			{
				UserId = Guid.NewGuid().ToString();
			}

			if (!string.IsNullOrEmpty(ConfigBaseUrl))
				ConfigBaseUrl = ConfigBaseUrl.TrimEnd('/');
		}
	}
}
