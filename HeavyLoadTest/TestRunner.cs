using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using AppMetrics.Client;

namespace HeavyLoadTest
{
	class TestRunner : MarshalByRefObject
	{
		public long Execute(string url)
		{
			try
			{
				var tracker = new Tracker(url, "HeavyLoadTest");
				for (int i = 0; i < LoopsCount; i++)
				{
					tracker.Log("Counter", i);
					tracker.Log("RandomValue", Guid.NewGuid().ToString());
					tracker.Log("RandomValue2", DateTime.Now.Millisecond);
					Thread.Sleep(1);
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
			Tracker.Terminate(true);
			var res = Tracker.GetServedRequestsCount();
			return res;
		}

		private const int LoopsCount = 100000;
	}
}
