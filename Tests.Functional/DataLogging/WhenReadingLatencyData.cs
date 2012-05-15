using System;
using System.Collections.Generic;
using System.Net;
using AppMetrics.Client;
using NUnit.Framework;

namespace Tests.DataLogging
{
	[TestFixture]
	public class WhenReadingLatencyData : IntegrationTestsBase
	{
		private string _appKey;
		private string[] _sessions;

		[TestFixtureSetUp]
		public void LogThenReadSomeLatencyData()
		{
			_appKey = GetType().FullName;

			var tracker = new Tracker(NormalizeUrl("LogEvent.ashx"), _appKey);
			tracker.Log("TestMessage", "TestValue");
			Tracker.Terminate(true);

			using (var client = new WebClient())
			{
				client.Credentials = new NetworkCredential(TestSettings.Instance.UserName, TestSettings.Instance.Password);
				client.QueryString["AppKey"] = _appKey;
				client.QueryString["Period"] = TimeSpan.FromSeconds(30).ToString();

				var response = client.DownloadString(TestSettings.Instance.SessionsExportUrl);
				_sessions = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		[Test]
		public void Then_a_new_session_should_have_been_created()
		{
			Assert.IsTrue(_sessions.Length > 0);
		}

		[Test]
		public void Then_a_new_record_should_have_been_created()
		{
			//TODO check that a new record has been created
		}
	}
}