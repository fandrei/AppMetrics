using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavyLoadTest
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length != 1)
					throw new ApplicationException("Invalid args");
				_url = args[0];

				var watch = Stopwatch.StartNew();

				var requestsSent = RunMultipleTests();

				watch.Stop();
				var secs = watch.Elapsed.TotalSeconds;
				Console.WriteLine("Requests sent: {0} in {1} secs ({2} per sec)", requestsSent, secs, requestsSent / secs);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}

		private static long RunMultipleTests()
		{
			var proxyType = typeof(TestRunner);
			var sync = new object();
			long res = 0;

			Parallel.For(0, ThreadsCount,
				i =>
				{
					try
					{
						var domain = AppDomain.CreateDomain("TestRunner" + i);
						var proxy = (TestRunner) domain.CreateInstanceAndUnwrap(proxyType.Assembly.FullName, proxyType.FullName);
						var subRes = proxy.Execute(_url);
						Console.WriteLine("Thread result: {0}", subRes);
						lock (sync)
						{
							res += subRes;
						}
					}
					catch (Exception exc)
					{
						Console.WriteLine(exc);
					}
				});

			return res;
		}

		private const int ThreadsCount = 32;
		private static string _url;
	}
}
