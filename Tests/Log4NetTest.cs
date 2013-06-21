using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using NUnit.Framework;

using AppMetrics.Client;
using AppMetrics.Client.Log4Net;

namespace Tests
{
	[TestFixture]
	class Log4NetTest
	{
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
					ApplicationKey = GetType().FullName,
				};
			appender.ActivateOptions();

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
			Log.Debug("TestMessage.Log4Net");
		}

		[TearDown]
		public void Cleanup()
		{
			Tracker.Terminate(true);
		}

		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Log4NetTest));
	}
}
