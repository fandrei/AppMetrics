﻿using System;
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
				if (args.Length != 1)
					throw new ApplicationException("Invalid args");
				var url = args[0];
				var tracker = new Tracker(url);

				tracker.Log("SomeValue", DateTime.Now.Millisecond);
				tracker.Log("SomeValue2", DateTime.Now.Millisecond);
				tracker.Log("SomeValue3", Guid.NewGuid().ToString());
				tracker.Log("SomeValue4", "aaa\r\nbbb\r\nccc");

				Tracker.Terminate();
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}
	}
}
