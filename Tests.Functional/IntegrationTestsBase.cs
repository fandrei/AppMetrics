using System;
using System.Diagnostics;
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

		protected void StartWebServer()
		{
			try
			{
				var basePath = FindAppMetricsPath();
				Trace.WriteLine(string.Format("Starting CassiniDev server ({0})", basePath));
				_server.StartServer(basePath, "/AppMetrics");
				return;
			}
			catch (InvalidOperationException operationException)
			{
				if (operationException.Message.Contains("Server already started"))
					return;

				throw;
			}
		}

		private static string FindAppMetricsPath()
		{
			var serviceRootFolder = TestSettings.Instance.ServiceRootFolder;
			if (String.IsNullOrEmpty(serviceRootFolder))
			{
				throw new ApplicationException(
					"Unable to find the TestSettings.Instance.ServiceRootFolder. "
				   + @"Make sure that you have configured the AppMetricsTest_ServiceRootFolder environment variable to point to the folder location of the AppMetrics website - eg C:\Dev\fandrei\AppMetrics\AppMetrics");
			}
			var p = Path.GetFullPath(serviceRootFolder);
			if (!Directory.Exists(p))
			{
				throw new ApplicationException(
					string.Format("Cannot find {0}.  Make sure that your unit test runner is not copying the tests to a shadow folder", p));
			}
			return p;
		}

		protected void StopWebServer()
		{
			Trace.WriteLine("Stopping CassiniDev server");
			_server.StopServer();
		}

		protected string NormalizeUrl(string relativeUrl)
		{
			return _server.NormalizeUrl(relativeUrl);
		}

	}
}