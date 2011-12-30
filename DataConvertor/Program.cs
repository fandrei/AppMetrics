using System;
using System.Collections.Generic;
using System.Diagnostics;
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
				var watch = Stopwatch.StartNew();

				var records = DataSource.GetRecordsFromPath(dataPath, DateTime.Now - DateTime.MinValue);

				Console.WriteLine("Parsing: {0} secs", watch.Elapsed.TotalSeconds);

				var latencies = (from record in records
								 where record.Name.StartsWith("Latency")
								 select decimal.Parse(record.Value)).ToList();

				Console.WriteLine("Preparing data: {0} secs", watch.Elapsed.TotalSeconds);

				decimal median, lowerQuartile, upperQuartile, min, max;
				Stats.CalculateSummaries(latencies, out median, out lowerQuartile, out upperQuartile, out min, out max);

				Console.WriteLine("Finding statistic summaries: {0} secs", watch.Elapsed.TotalSeconds);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}
	}
}
