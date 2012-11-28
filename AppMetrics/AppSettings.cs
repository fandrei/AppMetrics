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
		public bool RequireAccessKey { get; set; }
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