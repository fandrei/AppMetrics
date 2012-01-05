using System;
using System.Collections.Generic;
using System.Net;
using AppMetrics.Client;
using NUnit.Framework;
using Tests.AppMetricsDataService;

namespace Tests
{
    [TestFixture]
    public class WhenReadingLatencyData : IntegrationTestsBase
    {
        private string _appKey;
        private List<Session> _sessions;
        private List<Record> _records;

        [TestFixtureSetUp]
        public void LogThenReadSomeLatencyData()
        {
            _appKey = GetType().FullName;

            var tracker = new Tracker(NormalizeUrl("LogEvent.ashx"), _appKey);
            tracker.Log("TestMessage", "TestValue");
            Tracker.Terminate(true);
       
            var dataSource = new DataSource(new Uri(NormalizeUrl("/DataService.svc")))
                                 {
                                     Credentials = new NetworkCredential(TestSettings.Instance.UserName, TestSettings.Instance.Password)
                                 };

            _sessions = new List<Session>(
                dataSource.Sessions
                    .AddQueryOption("appKey", _appKey)
                    .AddQueryOption("period", TimeSpan.FromSeconds(30)));
           // _records = new List<Record>(dataSource.Records);
        }
        [Test]
        public void Then_a_new_session_should_have_been_created()
        {
            Assert.IsTrue(_sessions.Count > 0);
        }

        [Test]
        public void Then_a_new_record_should_have_been_created()
        {
          //  Assert.That(_records.Count, Is.GreaterThan(1));
        }
    }
}