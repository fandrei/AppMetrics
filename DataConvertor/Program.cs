using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using AppMetrics.Analytics;

namespace AppMetrics.DataConvertor
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length == 0 || args.Length > 3)
					throw new ApplicationException("Invalid command line args");

				var listeners = new[] { new TextWriterTraceListener(Console.Out) };
				Debug.Listeners.AddRange(listeners);

				var dataPath = args[0];
				var resPath = (args.Length >= 2) ? args[1] : ".";
				var periodDays = (args.Length >= 3) ? TimeSpan.Parse(args[2]) : TimeSpan.MaxValue;

				var convertor = new Convertor();
				convertor.Process(dataPath, resPath, periodDays);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}
	}
}
