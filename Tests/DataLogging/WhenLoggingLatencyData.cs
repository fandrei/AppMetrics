using AppMetrics.Client;
using NUnit.Framework;

namespace Tests.DataLogging
{
    [TestFixture]
    public class WhenLoggingLatencyData : IntegrationTestsBase
    {
        [TestFixtureSetUp]
        public void LogSomeLatencyData()
        {
            var appKey = GetType().FullName;

            var tracker = new Tracker(NormalizeUrl("LogEvent.ashx"), appKey);
            tracker.Log("TestMessage", "TestValue");
            Tracker.Terminate(true);
                
        }
        [Test]
        public void Then_a_new_session_txt_file_should_be_created()
        {
            //TODO check that the session text file is in App_data/AppKey/
        }

        [Test]
        public void The_the_session_txt_file_should_contain_client_info()
        {
            //TODO read the session text file and confirm that the client info logged looks good
        }
    }
}