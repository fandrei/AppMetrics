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
			var watch = Stopwatch.StartNew();

			var thread = new Thread(
				state => RunMultipleTests());
			thread.Start();

			Console.ReadKey();

			_terminate = true;
			thread.Join();

			watch.Stop();
			var secs = watch.Elapsed.TotalSeconds;
			Console.WriteLine("Resuests sent: {0} in {1} secs ({2} per sec)", _requestsSent, secs, _requestsSent / secs);
		}

		private static void RunMultipleTests()
		{
			Parallel.For(0, ThreadsCount,
						 i => RunTest());
		}

		static void RunTest()
		{
			var tracker = new Tracker();
			while (!_terminate)
			{
				tracker.Log("CurTime", DateTime.Now.ToString());
				CountNewRequest();
				tracker.Log("RandomValue", Guid.NewGuid().ToString());
				CountNewRequest();
			}
		}

		static void CountNewRequest()
		{
			Interlocked.Increment(ref _requestsSent);
		}

		static long _requestsSent;
		private static volatile bool _terminate;
		private const int ThreadsCount = 100;
	}
}
