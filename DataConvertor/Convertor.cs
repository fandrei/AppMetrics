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

			var res = CalculateLatencyInfo();
			WriteStatSummariesReport(res, resPath);
			WriteDistributionReport(res, resPath);
		}

		private List<CalcResult> CalculateLatencyInfo()
		{
			var watch = Stopwatch.StartNew();
			var res = new List<CalcResult>();

			var sessionsByCountries = GroupBy(_sessions, session => session.Location.countryName);

			{
				var allRecords = GetRecords(_sessions);
				var overallSummariesByFunction = CalculateByFunction(allRecords);
				res.AddRange(overallSummariesByFunction);
			}

			foreach (var pair in sessionsByCountries)
			{
				var countryName = pair.Key;

				var records = GetRecords(pair.Value);

				var curSummaries = CalculateByCities(records);
				foreach (var summary in curSummaries)
				{
					summary.Country = countryName;
				}

				res.AddRange(curSummaries);
				GC.Collect();
			}

			Console.WriteLine("Finding statistic summaries: {0} secs", watch.Elapsed.TotalSeconds);
			watch.Stop();

			return res;
		}

		private static List<CalcResult> CalculateByCities(ICollection<RecordEx> records)
		{
			var res = new List<CalcResult>();

			var tmp = CalculateByFunction(records);
			res.AddRange(tmp);

			var recordsByCities = GroupBy(records, record => (record.Session.Location.city) ?? "");
			recordsByCities.Remove("");
			foreach (var pair in recordsByCities)
			{
				var cityName = pair.Key;
				if (string.IsNullOrEmpty(cityName))
					continue;

				var curSummaries = CalculateByFunction(pair.Value);
				foreach (var summary in curSummaries)
				{
					summary.City = cityName;
				}
				res.AddRange(curSummaries);
			}

			return res;
		}

		private static List<CalcResult> CalculateByFunction(ICollection<RecordEx> records)
		{
			var res = new List<CalcResult>();

			var tmp = Calculate(records);
			res.Add(tmp);

			var recordsByFunction = GroupBy(records, record => record.Name.Split(' ')[1]);
			foreach (var pair in recordsByFunction)
			{
				var functionName = pair.Key;
				if (string.IsNullOrEmpty(functionName))
					throw new ApplicationException();

				var curSummary = Calculate(pair.Value);
				curSummary.FunctionName = functionName;
				res.Add(curSummary);
			}

			return res;
		}

		private static CalcResult Calculate(ICollection<RecordEx> records)
		{
			var res = new CalcResult();
			res.StatSummary = CalculateStatSummary(records);
			res.Distribution = CalculateDistribution(records);
			return res;
		}

		private static StatSummary CalculateStatSummary(IEnumerable<RecordEx> records)
		{
			var latencies = (from record in records
							 select decimal.Parse(record.Value)).ToList();

			var res = Stats.CalculateSummaries(latencies);
			return res;
		}

		private static Distribution CalculateDistribution(ICollection<RecordEx> records)
		{
			var res = new Distribution { Count = records.Count };

			var latencies = (from record in records
							 select decimal.Parse(record.Value)).ToArray();

			foreach (var latency in latencies)
			{
				var rounded = Math.Ceiling(latency);
				if (res.Vals.ContainsKey(rounded))
					res.Vals[rounded]++;
				else
					res.Vals[rounded] = 1;
			}

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

		private static void WriteStatSummariesReport(IEnumerable<CalcResult> results, string resPath)
		{
			resPath = Path.GetFullPath(resPath + "\\LatencyStatSummaries.txt");

			using (var file = new StreamWriter(resPath, false, Encoding.UTF8))
			{
				file.WriteLine("Country\tCity\tFunctionName\tCount\tAverage\tMin\tLowerQuartile\tMedian\tUpperQuartile\tMax");

				foreach (var result in results)
				{
					var summary = result.StatSummary;
					file.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
						result.Country, result.City, result.FunctionName,
						summary.Count, summary.Average,
						summary.Min, summary.LowerQuartile, summary.Median, summary.UpperQuartile, summary.Max);
				}
			}
		}

		private static void WriteDistributionReport(IEnumerable<CalcResult> results, string resPath)
		{
			resPath = Path.GetFullPath(resPath + "\\LatencyDistribution.txt");

			using (var file = new StreamWriter(resPath, false, Encoding.UTF8))
			{
				file.WriteLine("Country\tCity\tFunctionName\tLatency\tCount");

				foreach (var result in results)
				{
					foreach (var pair in result.Distribution.Vals)
					{
						file.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}",
						result.Country, result.City, result.FunctionName,
						pair.Key, pair.Value);
					}
				}
			}
		}

		private List<SessionEx> _sessions;
	}
}
