using System;
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
            StartWebServer(@"AppMetrics");
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            StopWebServer();
        }

        protected void StartWebServer(string websiteFolder)
        {
            try
            {
                if (IsRunByReSharperUnitTestRunner())
                {
                    _server.StartServer(@"..\..\..\..\..\..\" + websiteFolder, "/AppMetrics");
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

        private bool IsRunByReSharperUnitTestRunner()
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