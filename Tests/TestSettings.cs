using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using AppMetrics.Shared;

namespace Tests
{
	public class TestSettings : AppSettingsBase
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

		protected override void OnAfterLoad()
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

		public static TestSettings Load()
		{
			var fileName = Util.GetAppLocation() + @"\TestSettings.xml";
			var settings = Load<TestSettings>(fileName);

			Console.WriteLine("### Loaded settings ###");
			Console.WriteLine(settings.ServiceRootFolder);
			Console.WriteLine(settings.DataFolder);
			Console.WriteLine(settings.UserName);
			Console.WriteLine("######");

			return settings;
		}

		#endregion
	}
}
