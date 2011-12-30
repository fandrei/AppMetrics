using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AppMetrics.DataModel;

namespace AppMetrics.DataConvertor
{
	class Convertor
	{
		public void Process(string dataPath)
		{
			PrepareData(dataPath);
			GC.Collect();

			var watch = Stopwatch.StartNew();

			var latencies = (from record in _records
							 where record.Name.StartsWith("Latency")
							 select decimal.Parse(record.Value)).ToList();

			decimal median, lowerQuartile, upperQuartile, min, max;
			Stats.CalculateSummaries(latencies, out median, out lowerQuartile, out upperQuartile, out min, out max);

			Console.WriteLine("Finding statistic summaries: {0} secs", watch.Elapsed.TotalSeconds);
		}

		private void PrepareData(string dataPath)
		{
			ParseData(dataPath);

			var watch = Stopwatch.StartNew();

			var geoDataPath = Path.GetFullPath(@"..\..\tools\GeoIP\GeoLiteCity.dat");
			var geoLookup = new LookupService(geoDataPath, LookupService.GEOIP_MEMORY_CACHE);

			foreach (var session in _sessions)
			{
				session.Ip = session.Records.Find(record => record.Name == "ClientIP").Value;
				session.Location = geoLookup.getLocation(session.Ip);
			}

			Console.WriteLine("Preparing data: {0} secs", watch.Elapsed.TotalSeconds);
		}

		private void ParseData(string dataPath)
		{
			var watch = Stopwatch.StartNew();

			_records = new List<RecordEx>();
			_sessions = new List<SessionEx>();

			var sessions = DataSource.GetSessionsFromPath(dataPath, DateTime.Now - DateTime.MinValue);
			foreach (var session in sessions)
			{
				var records = DataSource.GetRecordsFromSession(session);

				var sessionEx = new SessionEx
					{
						Id = session.Id,
						CreationTime = session.CreationTime,
						LastUpdateTime = session.LastUpdateTime
					};
				_sessions.Add(sessionEx);

				sessionEx.Records = records.ConvertAll(
					val => new RecordEx(sessionEx)
						{
							SessionId = val.SessionId,
							Name = val.Name,
							Time = val.Time,
							Value = val.Value
						});
				_records.AddRange(sessionEx.Records);
			}

			Console.WriteLine("Parsing data: {0} secs", watch.Elapsed.TotalSeconds);
		}

		private List<RecordEx> _records;
		private List<SessionEx> _sessions;
	}
}
