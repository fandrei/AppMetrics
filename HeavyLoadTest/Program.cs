using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AppMetrics.Client;

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

				var thread = new Thread(
					state => RunMultipleTests());
				thread.Start();

				Console.ReadKey();

				_terminate = true;
				thread.Join();
				Tracker.Terminate();

				watch.Stop();
				var secs = watch.Elapsed.TotalSeconds;
				Console.WriteLine("Requests sent: {0} in {1} secs ({2} per sec)", _requestsSent, secs, _requestsSent/secs);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}

		private static void RunMultipleTests()
		{
			Parallel.For(0, ThreadsCount,
						 i => RunTest());
		}

		static void RunTest()
		{
			try
			{
				var tracker = new Tracker(_url);
				while (!_terminate)
				{
					tracker.Log("RandomValue", Guid.NewGuid().ToString());
					CountNewRequest();
					tracker.Log("RandomValue2", DateTime.Now.Millisecond);
					CountNewRequest();
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}

		static void CountNewRequest()
		{
			Interlocked.Increment(ref _requestsSent);
		}

		static long _requestsSent;
		private static volatile bool _terminate;
		private const int ThreadsCount = 100;
		private static string _url;
	}
}
