using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AppMetrics.Client;

namespace DebugProject
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var tracker = new Tracker();
				tracker.Log("CurTime", DateTime.Now.ToString());
				tracker.Log("SomeValue", DateTime.Now.Millisecond);
				tracker.Log("SomeValue2", DateTime.Now.Millisecond);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}
	}
}
