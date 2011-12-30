using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AppMetrics.DataConvertor
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length != 2)
					throw new ApplicationException("Invalid command line args");

				var dataPath = args[0];
				var resPath = args[1];

				var convertor = new Convertor();
				convertor.Process(dataPath, resPath);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}
	}
}
