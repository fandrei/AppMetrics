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
		public void Process(string dataPath, string resPath)
		{
			ReadData(dataPath);
			GC.Collect();

			CalculateLatencySummaries(resPath + "\\LatencyStatSummaries.txt");
			GC.Collect();
		}

		private void CalculateLatencySummaries(string resPath)
		{
			var watch = Stopwatch.StartNew();
			var res = new List<StatSummary>();

			var sessionsByCountries = GroupBy(_sessions, session => session.Location.countryName);

			{
				var allRecords = GetRecords(_sessions);
				var overallSummariesByFunction = CalculateSummariesByFunction(allRecords);
				res.AddRange(overallSummariesByFunction);
			}

			foreach (var pair in sessionsByCountries)
			{
				var countryName = pair.Key;

				var records = GetRecords(pair.Value);

				var curSummaries = CalculateSummariesByCities(records);
				foreach (var summary in curSummaries)
				{
					summary.Country = countryName;
				}

				res.AddRange(curSummaries);
				GC.Collect();
			}

			Console.WriteLine("Finding statistic summaries: {0} secs", watch.Elapsed.TotalSeconds);
			watch.Stop();

			WriteReport(res, resPath);
		}

		private static void WriteReport(IEnumerable<StatSummary> summaries, string resPath)
		{
			resPath = Path.GetFullPath(resPath);
			using (var file = new StreamWriter(resPath, false, Encoding.UTF8))
			{
				file.WriteLine("Country\tCity\tFunctionName\tCount\tAverage\tMin\tLowerQuartile\tMedian\tUpperQuartile\tMax");

				foreach (var summary in summaries)
				{
					file.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
						summary.Country, summary.City, summary.FunctionName,
						summary.Count, summary.Average,
						summary.Min, summary.LowerQuartile, summary.Median, summary.UpperQuartile, summary.Max);
				}
			}
		}

		private static List<StatSummary> CalculateSummariesByCities(IEnumerable<RecordEx> records)
		{
			var res = new List<StatSummary>();

			var tmp = CalculateSummariesByFunction(records);
			res.AddRange(tmp);

			var recordsByCities = GroupBy(records, record => (record.Session.Location.city) ?? "");
			recordsByCities.Remove("");
			foreach (var pair in recordsByCities)
			{
				var cityName = pair.Key;
				if (string.IsNullOrEmpty(cityName))
					continue;

				var curSummaries = CalculateSummariesByFunction(pair.Value);
				foreach (var summary in curSummaries)
				{
					summary.City = cityName;
				}
				res.AddRange(curSummaries);
			}

			return res;
		}

		private static List<StatSummary> CalculateSummariesByFunction(IEnumerable<RecordEx> records)
		{
			var res = new List<StatSummary>();

			var tmp = CalculateStatSummary(records);
			res.Add(tmp);

			var recordsByFunction = GroupBy(records, record => record.Name.Split(' ')[1]);
			foreach (var pair in recordsByFunction)
			{
				var functionName = pair.Key;
				if (string.IsNullOrEmpty(functionName))
					throw new ApplicationException();

				var curSummary = CalculateStatSummary(pair.Value);
				curSummary.FunctionName = functionName;
				res.Add(curSummary);
			}

			return res;
		}

		private static StatSummary CalculateStatSummary(IEnumerable<RecordEx> records)
		{
			var latencies = (from record in records
							 select decimal.Parse(record.Value)).ToList();

			var res = Stats.CalculateSummaries(latencies);
			return res;
		}

		private void ReadData(string dataPath)
		{
			ParseData(dataPath);

			var watch = Stopwatch.StartNew();

			var geoDataPath = Path.GetFullPath(@"..\..\tools\GeoIP\GeoLiteCity.dat");
			var geoLookup = new LookupService(geoDataPath, LookupService.GEOIP_MEMORY_CACHE);

			foreach (var session in _sessions)
			{
				session.Ip = session.Records.Find(record => record.Name == "ClientIP").Value;
				session.Location = geoLookup.getLocation(session.Ip);

				// leave only latency info
				session.Records.RemoveAll(record => !record.Name.StartsWith("Latency"));
			}

			Console.WriteLine("Preparing data: {0} secs", watch.Elapsed.TotalSeconds);
		}

		private void ParseData(string dataPath)
		{
			var watch = Stopwatch.StartNew();

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
			}

			Console.WriteLine("Parsing data: {0} secs", watch.Elapsed.TotalSeconds);
		}

		static Dictionary<TKey, List<TSource>> GroupBy<TSource, TKey>(IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector)
		{
			var res = source.GroupBy(keySelector).ToDictionary(pair => pair.Key, pair => pair.ToList());
			return res;
		}

		private static List<RecordEx> GetRecords(IEnumerable<SessionEx> sessions)
		{
			var records = new List<RecordEx>();
			foreach (var session in sessions)
			{
				records.AddRange(session.Records);
			}
			return records;
		}

		private List<SessionEx> _sessions;
	}
}
