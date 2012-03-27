using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml.Serialization;

namespace AppMetrics
{
	public class AppSettings
	{
		#region Site config

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

		#endregion

		#region Shared settings

		public string AmazonAccessKey { get; set; }

		public string AmazonSecretAccessKeyEncrypted { get; set; }

		[XmlIgnore]
		public string AmazonSecretAccessKey
		{
			get
			{
				if (string.IsNullOrEmpty(AmazonSecretAccessKeyEncrypted))
					return null;

				var encryptedBytes = Convert.FromBase64String(AmazonSecretAccessKeyEncrypted);
				var data = ProtectedData.Unprotect(encryptedBytes, AdditionalEntropy, DataProtectionScope.LocalMachine);
				var res = Encoding.UTF8.GetString(data);
				return res;
			}
			set
			{
				var data = Encoding.UTF8.GetBytes(value);
				var encrypted = ProtectedData.Protect(data, AdditionalEntropy, DataProtectionScope.LocalMachine);
				AmazonSecretAccessKeyEncrypted = Convert.ToBase64String(encrypted);
			}
		}

		static readonly byte[] AdditionalEntropy = { 0xF0, 0x3A, 0xDD, 0x14, 0xCB, 0x7C, 0x4A, 0xCC, 0x9E, 0x8A };

		private static AppSettings _instance;

		public static AppSettings Instance
		{
			get { return _instance ?? (_instance = Load()); }
		}

		private static readonly string FileName = DataStoragePath + "\\settings.xml";

		public static void Reload()
		{
			_instance = Load();
		}

		public static AppSettings Load()
		{
			AppSettings settings;

			if (File.Exists(FileName))
			{
				var s = new XmlSerializer(typeof(AppSettings));
				using (var rd = new StreamReader(FileName))
				{
					settings = (AppSettings)s.Deserialize(rd);
				}
			}
			else
				settings = new AppSettings();

			return settings;
		}

		public void Save()
		{
			var directory = Path.GetDirectoryName(FileName);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			var s = new XmlSerializer(typeof(AppSettings));
			using (var writer = new StreamWriter(FileName))
			{
				s.Serialize(writer, this);
			}
		}
		#endregion
	}
}