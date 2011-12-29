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

				var latencies = new List<decimal>();

				foreach (var record in records)
				{
					if (record.Name.StartsWith("Latency"))
					{
						var val = decimal.Parse(record.Value);
						latencies.Add(val);
					}
				}

				decimal median, lowerQuartile, upperQuartile;
				Stats.FindQuartiles(latencies, out median, out lowerQuartile, out upperQuartile);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}
	}
}
