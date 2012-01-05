using System;
using System.IO;
using System.Linq;
using CassiniDev;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class IntegrationTestsBase
    {
        private readonly CassiniDevServer _server = new CassiniDevServer();

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            StartWebServer();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            StopWebServer();
        }

        protected void StartWebServer()
        {
            try
            {
                if (IsRunByReSharperUnitTestRunner())
                {
                    _server.StartServer(FindAppMetricsPath(), "/AppMetrics");
                    return;
                }
            }
            catch (InvalidOperationException operationException)
            {
                if (operationException.Message.Contains("Server already started"))
                    return;

                throw;
            }

            throw new Exception("Unrecognised unit test runner - currently only support running via Resharper runner");
        }

        private static string FindAppMetricsPath()
        {
            var p = Path.GetFullPath(@"..\..\AppMetrics");
            if (!Directory.Exists(p))
            {
                throw new ApplicationException(
                    string.Format("Cannot find {0}.  Make sure that your unit test runner is not copying the tests to a shadow folder", p));
            }
            return p;
        }

        private static bool IsRunByReSharperUnitTestRunner()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.FullName.StartsWith("JetBrains.ReSharper.TaskRunnerFramework"));
        }

       protected void StopWebServer()
        {
            _server.StopServer();
        }

        protected string NormalizeUrl(string relativeUrl)
        {
            return _server.NormalizeUrl(relativeUrl);
        }

    }
}