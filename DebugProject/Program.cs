using System;
using System.Collections.Generic;
using System.Diagnostics;
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
				if (args.Length != 1)
					throw new ApplicationException("Invalid args");
				var url = args[0];

				var listeners = new[] { new TextWriterTraceListener(Console.Out) };
				Debug.Listeners.AddRange(listeners);

				var tracker = new Tracker(url, "DebugProject");

				tracker.Log("SomeValue", DateTime.Now.Millisecond);
				tracker.Log("SomeValue2", DateTime.Now.Millisecond);
				tracker.Log("SomeValue3", Guid.NewGuid().ToString());
				tracker.Log("SomeValue4", "aaa\r\nbbb\r\nccc");

				for (int i = 0; i < 20; i++)
				{
					tracker.Log(string.Format("SomeRandomValue{0}", i), Guid.NewGuid().ToString());
				}

				Tracker.Terminate(true);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}
	}
}
