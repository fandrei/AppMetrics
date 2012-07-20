using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace AppMetrics
{
	public class AppSettings
	{
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

		public string[] AccessKeys { get; set; }

		private static AppSettings _instance;

		public static AppSettings Instance
		{
			get { return _instance; }
		}

		private static string FileName;

		public static void Load(string dataPath)
		{
			FileName = dataPath + "\\settings.xml";
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

			_instance = settings;
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
	}
}