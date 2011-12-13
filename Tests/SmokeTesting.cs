using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using NUnit.Framework;

using AppMetrics.Client;
using Tests.AppMetricsDataService;

namespace Tests
{
	[TestFixture]
	public class SmokeTesting
	{
		private const string AppKey = "SmokeTest";
		private const string RequestPeriod = "0:5:0";

		[Test]
		public void SmokeTest()
		{
			var tracker = new Tracker(AppSettings.Instance.MetricsLoggingUrl, AppKey);
			tracker.Log("TestMessage", "TestValue");

			Tracker.Terminate(true);

			var dataSource = new DataSource(new Uri(AppSettings.Instance.MetricsExportUrl))
				{
					Credentials = new NetworkCredential(AppSettings.Instance.UserName, AppSettings.Instance.Password)
				};

			var sessions = new List<Session>(
				dataSource.Sessions.AddQueryOption("appKey", AppKey).AddQueryOption("period", RequestPeriod));
			Assert.IsTrue(sessions.Count > 0);
		}
	}
}
