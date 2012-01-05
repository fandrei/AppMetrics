using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using Tests.AppMetricsDataService;

namespace Tests
{
    [TestFixture]
    public class DataLoggingTests 
    {
        [TestFixture]
        public class WhenLoggingLatencyData : IntegrationTestsBase
        {
            [TestFixtureSetUp]
            public void LogSomeLatencyData()
            {
                Console.WriteLine("LogSomeLatencyData");
                var dataSource = new DataSource(new Uri(NormalizeUrl("/DataService.svc/")))
				{
					Credentials = new NetworkCredential(TestSettings.Instance.UserName, TestSettings.Instance.Password)
				};

			    var sessions = new List<Session>(
				    dataSource.Sessions.AddQueryOption("appKey", "akey").AddQueryOption("period", TimeSpan.FromSeconds(30)));
			    Assert.IsTrue(sessions.Count > 0);
                
                
            }
            [Test]
            public void ShouldDoIt()
            {
                Console.WriteLine("ShouldDoIt");

                
            }

            [Test]
            public void ShouldDoIt2()
            {
                Console.WriteLine("ShouldDoIt");
            }
        }

        [TestFixture]
        public class WhenReadingLatencyData : IntegrationTestsBase
        {
            [TestFixtureSetUp]
            public void LogSomeLatencyData()
            {
                Console.WriteLine("LogSomeLatencyData");

            }
            [Test]
            public void ShouldDoIt()
            {
                Console.WriteLine("ShouldDoIt");

            }

            [Test]
            public void ShouldDoIt2()
            {
                Console.WriteLine("ShouldDoIt");

            }
        }
//        /// <summary>
//        /// Unignore when we can include the HttpStatusCode in the ErrorResponseDTO
//        /// </summary>
//        [Test]
//        [DeploymentItem(path: "RESTWebservices.MVC\\RESTWebservices.MVC.Default",
//             outputDirectory: "RESTWebservices.MVC.Default")]
//        public void WhenNoSessionPassedUnauthorizedResponseReturned()
//        {
//            var response =
//                GetUnAuthenticatedHttpClient()
//                .Get(NormalizeUrl("/test/authenticated"));
//
//            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
//        }


    }
}

