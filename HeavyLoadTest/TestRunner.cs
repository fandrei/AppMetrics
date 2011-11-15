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
				var tracker = new Tracker(url);
				for (int i = 0; i < LoopsCount; i++)
				{
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
			return Tracker.GetServedRequestsCount();
		}

		private const int LoopsCount = 1000;
	}
}
