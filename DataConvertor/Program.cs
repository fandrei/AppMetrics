using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AppMetrics.DataModel;

namespace AppMetrics.DataConvertor
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length != 1)
					throw new ApplicationException("Invalid command line args");

				var dataPath = args[0];

				var records = DataSource.GetRecordsFromPath(dataPath, DateTime.Now - DateTime.MinValue);

				var latencies = (from record in records
								 where record.Name.StartsWith("Latency")
								 select decimal.Parse(record.Value)).ToList();

				decimal median, lowerQuartile, upperQuartile, min, max;
				Stats.CalculateSummaries(latencies, out median, out lowerQuartile, out upperQuartile, out min, out max);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}
	}
}
