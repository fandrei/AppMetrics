using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AppMetrics.Client;
using NUnit.Framework;

namespace Tests.DataLogging
{
	[TestFixture]
	public class WhenLoggingLatencyData : IntegrationTestsBase
	{
		private string _logField = "WhenLoggingLatencyData_Field";
		private string _logValue = DateTime.UtcNow.ToLongTimeString();
		private string _appKey;

		[TestFixtureSetUp]
		public void LogSomeLatencyData()
		{
			Trace.WriteLine(string.Format("Starting test {0}", GetType().FullName));

			StartWebServer();

			_appKey = GetType().FullName;

			var tracker = Tracker.Create(NormalizeUrl("LogEvent.ashx"), _appKey, TestSettings.Instance.AccessKey);
			tracker.Log(_logField, _logValue);
			tracker.FlushMessages();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			StopWebServer();
		}

		[Test]
		public void Then_a_new_session_txt_file_should_be_created()
		{
			Assert.That(GetSessionFiles().Length, Is.GreaterThan(0));
		}

		[Test]
		public void Then_the_session_txt_file_should_contain_client_info()
		{
			var files = GetSessionFiles();
			var sessionData = File.ReadAllLines(files.Last());

			Assert.That(ContainsLogEntry(sessionData, "ClientIP", "127.0.0.1"), Is.True);
			Assert.That(ContainsLogEntry(sessionData, "ClientHostName", "127.0.0.1"), Is.True);
		}

		private static bool ContainsLogEntry(IEnumerable<string> sessionData, string key, string value)
		{
			return sessionData.AsQueryable().First(l => l.Contains(key)).Contains(value);
		}

		private string[] GetSessionFiles()
		{
			var sessionPath = Path.Combine(TestSettings.Instance.DataFolder, _appKey);
			return Directory.GetFiles(sessionPath, "*.txt");
		}
	}
}