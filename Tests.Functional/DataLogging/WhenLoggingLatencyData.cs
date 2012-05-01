using System;
using System.Collections.Generic;
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
            _appKey = GetType().FullName;

            var tracker = new Tracker(NormalizeUrl("LogEvent.ashx"), _appKey);
            tracker.Log(_logField, _logValue);
            Tracker.Terminate(true);
        }

        [Test]
        public void Then_a_new_session_txt_file_should_be_created()
        {
            Assert.That(GetSessionFiles().Length, Is.GreaterThan(0));
        }

        [Test]
        public void The_the_session_txt_file_should_contain_client_info()
        {
            var files = GetSessionFiles();
            var sessionData = File.ReadAllLines(files.Last());

            Assert.That(ContainsLogEntry(sessionData, "ClientIP","127.0.0.1"), Is.True);
            Assert.That(ContainsLogEntry(sessionData, "ClientHostName", "127.0.0.1"), Is.True);
        }

        private static bool ContainsLogEntry(IEnumerable<string> sessionData, string key, string value)
        {
            return sessionData.AsQueryable().First(l => l.Contains(key)).Contains(value);
        }

        private string[] GetSessionFiles()
        {
            return Directory.GetFiles(
                Path.Combine(TestSettings.Instance.ServiceRootFolder, _appKey), "*.txt");
        }
    }
}