using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Tests
{
	public class TestSettings
	{
		public string ServiceRootUrl { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }

		[XmlIgnore]
		public string MetricsLoggingUrl { get { return CombineUri(ServiceRootUrl, "LogEvent.ashx"); } }
		[XmlIgnore]
		public string MetricsExportUrl { get { return CombineUri(ServiceRootUrl, "DataService.svc/"); } }

		public static string CombineUri(string root, string tail)
		{
			if (string.IsNullOrEmpty(root))
				throw new ArgumentNullException();

			if (!root.EndsWith("/") && !tail.StartsWith("/"))
				root += "/";
			var res = root + tail;
			return res;
		}

		private void SetDefaultsIfEmpty()
		{
			if (string.IsNullOrEmpty(ServiceRootUrl))
				ServiceRootUrl = Environment.GetEnvironmentVariable("AppMetricsTest_ServiceRootUrl");

			if (string.IsNullOrEmpty(UserName))
				UserName = Environment.GetEnvironmentVariable("AppMetricsTest_UserName");

			if (string.IsNullOrEmpty(Password))
				Password = Environment.GetEnvironmentVariable("AppMetricsTest_Password");
		}

		#region Config storing implementation

		private static TestSettings _instance;

		public static TestSettings Instance
		{
			get { return _instance ?? (_instance = Load()); }
		}

		private static readonly string FileName = Util.GetAppLocation() + @"\TestSettings.xml";

		public static void Reload()
		{
			_instance = Load();
		}

		public static TestSettings Load()
		{
			TestSettings settings;

			if (File.Exists(FileName))
			{
				var s = new XmlSerializer(typeof(TestSettings));
				using (var rd = new StreamReader(FileName))
				{
					settings = (TestSettings)s.Deserialize(rd);
				}
			}
			else
				settings = new TestSettings();

			settings.SetDefaultsIfEmpty();

			return settings;
		}

		public void Save()
		{
			var directory = Path.GetDirectoryName(FileName);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			var s = new XmlSerializer(typeof(TestSettings));
			using (var writer = new StreamWriter(FileName))
			{
				s.Serialize(writer, this);
			}
		}

		#endregion
	}
}
