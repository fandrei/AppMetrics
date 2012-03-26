using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using System.Web.Hosting;

namespace AppMetrics
{
	public static class AppSettings
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

		public static string AmazonAccessKey
		{
			get { return Get("AmazonAccessKey"); }
		}

		static readonly byte[] AdditionalEntropy = { 0xF0, 0x3A, 0xDD, 0x14, 0xCB, 0x7C, 0x4A, 0xCC, 0x9E, 0x8A };

		public static string AmazonSecretAccessKey
		{
			get
			{
				var encrypted = Convert.FromBase64String(Get("AmazonSecretAccessKey"));
				var data = ProtectedData.Unprotect(encrypted, AdditionalEntropy, DataProtectionScope.LocalMachine);
				var res = Encoding.UTF8.GetString(data);
				return res;
			}
			set
			{
				var data = Encoding.UTF8.GetBytes(value);
				var encrypted = ProtectedData.Protect(data, AdditionalEntropy, DataProtectionScope.LocalMachine);
				Set("AmazonSecretAccessKey", Convert.ToBase64String(encrypted));
			}
		}

		static string Get(string name)
		{
			return Config.AppSettings.Settings[name].Value;
		}

		static void Set(string name, string value)
		{
			Config.AppSettings.Settings[name].Value = value;
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