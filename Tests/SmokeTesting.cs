using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using AppMetrics.Client;
using Tests.AppMetricsDataService;

namespace Tests
{
	[TestFixture]
	public class SmokeTesting
	{
		private const string ServiceRootUrl = "http://184.73.228.71";
		private const string MetricsLoggingUrl = ServiceRootUrl + "/AppMetrics/LogEvent.ashx";
		private const string MetricsExportUrl = ServiceRootUrl + "/AppMetrics/DataService.svc/";

		private const string AppKey = "SmokeTest";
		private const string RequestPeriod = "0:5:0";

		[Test]
		public void SmokeTest()
		{
			var tracker = new Tracker(MetricsLoggingUrl, AppKey);
			tracker.Log("TestMessage", "TestValue");

			Tracker.Terminate(true);

			var dataSource = new DataSource(new Uri(MetricsExportUrl));

			var sessions = new List<Session>(
				dataSource.Sessions.AddQueryOption("appKey", AppKey).AddQueryOption("period", RequestPeriod));
			Assert.IsTrue(sessions.Count > 0);
		}
	}
}
