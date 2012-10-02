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
		public string ServiceRootFolder { get; set; }
		public string ServiceRootUrl { get; set; }
		public string DataFolder { get; set; }

		public string UserName { get; set; }
		public string Password { get; set; }

		public string AccessKey { get; set; }

		[XmlIgnore]
		public string MetricsLoggingUrl { get { return CombineUri(ServiceRootUrl, "LogEvent.ashx"); } }
		[XmlIgnore]
		public string SessionsExportUrl { get { return CombineUri(ServiceRootUrl, "GetSessions.ashx"); } }

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
			if (string.IsNullOrEmpty(ServiceRootFolder))
				ServiceRootFolder = Environment.GetEnvironmentVariable("AppMetricsTest_ServiceRootFolder");

			if (string.IsNullOrEmpty(DataFolder))
				DataFolder = Environment.GetEnvironmentVariable("AppMetricsTest_DataFolder");

			if (string.IsNullOrEmpty(ServiceRootUrl))
				ServiceRootUrl = Environment.GetEnvironmentVariable("AppMetricsTest_ServiceRootUrl");

			if (string.IsNullOrEmpty(UserName))
				UserName = Environment.GetEnvironmentVariable("AppMetricsTest_UserName");

			if (string.IsNullOrEmpty(Password))
				Password = Environment.GetEnvironmentVariable("AppMetricsTest_Password");

			if (string.IsNullOrEmpty(AccessKey))
				AccessKey = Environment.GetEnvironmentVariable("AppMetricsTest_AccessKey");
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

			Trace.WriteLine("### Loaded settings ###");
			Trace.WriteLine(settings.ServiceRootFolder);
			Trace.WriteLine(settings.DataFolder);
			Trace.WriteLine("######");

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
