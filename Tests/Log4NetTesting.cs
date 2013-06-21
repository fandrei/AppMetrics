using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using NUnit.Framework;

using AppMetrics;
using AppMetrics.Client;
using AppMetrics.Client.Log4Net;

namespace Tests
{
	[TestFixture]
	class Log4NetTesting
	{
		private const string AppKey = "Tracking.Log4NetTesting";

		[SetUp]
		public void Init()
		{
			var layout = new PatternLayout("%utcdate %-5level - %message%newline");
			layout.ActivateOptions();

			var appender = new Log4NetAppender
				{
					Layout = layout,
					Server = TestSettings.Instance.MetricsLoggingUrl,
					AccessKey = TestSettings.Instance.AccessKey,
					ApplicationKey = AppKey,
				};
			appender.ActivateOptions();

			_tracker = appender.Tracker;

			var appender2 = new TraceAppender
				{
					Layout = layout,
				};

			BasicConfigurator.Configure(appender);
			BasicConfigurator.Configure(appender2);
		}

		[Test]
		public void SmokeTest()
		{
			var startTime = DateTime.UtcNow;

			var message = "TestMessage.Log4Net." + Guid.NewGuid().ToString();
			_log.Debug(message);

			_tracker.FlushMessages();

			var credentials = new NetworkCredential(TestSettings.Instance.UserName, TestSettings.Instance.Password);
			var args = new Dictionary<string, string>
				{
					{ "Application", AppKey }, 
					{ "StartTime", startTime.ToString("u") }
				};

			var response = HttpUtil.Request(TestSettings.Instance.RecordsExportUrl, credentials, args);

			Assert.IsTrue(response.Contains(message));
		}

		[TearDown]
		public void Cleanup()
		{
			_tracker.FlushMessages();
			_tracker.Dispose();
		}

		private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(Log4NetTesting));
		private TrackerBase _tracker;
	}
}
