using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using AppMetrics.Shared;
using NUnit.Framework;

using AppMetrics.Client;

namespace Tests
{
	[TestFixture]
	public class SmokeTesting
	{
		private const string AppKey = "Tracking.SmokeTest";
		private const string RequestPeriod = "0:5:0";

		[Test]
		public void SmokeTest()
		{
			Console.WriteLine("\r\nTesting service located at {0}\r\n", TestSettings.Instance.ServiceRootUrl);

			var startTime = DateTime.UtcNow;

			var tracker = Tracker.Create(TestSettings.Instance.MetricsLoggingUrl, AppKey);
			tracker.Log("TestMessage", "TestValue");
			tracker.FlushMessages();

			using (var client = new WebClient())
			{
				client.Credentials = new NetworkCredential(TestSettings.Instance.UserName, TestSettings.Instance.Password);
				client.QueryString["AppKey"] = AppKey;
				client.QueryString["StartTime"] = startTime.ToString("u");

				var response = client.DownloadString(TestSettings.Instance.SessionsExportUrl);
				var sessions = Session.Parse(response);

				Assert.IsTrue(sessions.Count() > 0);

				var thisSession = sessions.Find(session => session.Id == tracker.SessionId);
				Assert.IsTrue(thisSession != null);
			}
		}
	}
}
