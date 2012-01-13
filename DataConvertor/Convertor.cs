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
		public void Process(string dataPath, string resFolder)
		{
			ReadData(dataPath);
			GC.Collect();

			WriteSummaryReport(_sessions, resFolder);

			var res = CalculateByCountries();
			WriteStatSummariesReport(res, resFolder);
			WriteDistributionReport(res, resFolder);
		}

		private List<CalcResult> CalculateByCountries()
		{
			var watch = Stopwatch.StartNew();
			var res = new List<CalcResult>();

			{
				var allRecords = GetRecords(_sessions);
				var overallSummariesByFunction = CalculateByFunction(allRecords);
				foreach (var summary in overallSummariesByFunction)
				{
					summary.Country = "(World)";
				}
				res.AddRange(overallSummariesByFunction);
			}

			var sessionsByCountries = GroupBy(_sessions, session => session.Location.countryName);
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
			}

			Console.WriteLine("Finding statistic summaries: {0} secs", watch.Elapsed.TotalSeconds);
			watch.Stop();

			return res;
		}

		private static List<CalcResult> CalculateByCities(ICollection<RecordEx> records)
		{
			var res = new List<CalcResult>();

			{
				var tmp = CalculateByFunction(records);
				res.AddRange(tmp);
				foreach (var summary in tmp)
				{
					summary.City = "(All)";
				}
			}

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
			var latencies = records.Where(IsLatency).Select(record => record.ValueAsNumber).ToArray();
			if (latencies.Length > 0)
			{
				res.StatSummary = Stats.CalculateSummaries(latencies);
				res.Distribution = CalculateDistribution(latencies);
			}
			return res;
		}

		private static bool IsLatency(RecordEx record)
		{
			return record.Name.StartsWith("Latency");
		}

		private static bool IsJitter(RecordEx record)
		{
			return record.Name.StartsWith("Jitter");
		}

		private static Distribution CalculateDistribution(decimal[] latencies)
		{
			var res = new Distribution { Count = latencies.Length };

			foreach (var latency in latencies)
			{
				var rounded = Math.Ceiling(latency * 2) / 2; // find nearest ceiling with period of 0.5
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
				session.Records.RemoveAll(record => !IsLatency(record) && !IsJitter(record));

				foreach (var record in session.Records)
				{
					try
					{
						record.ValueAsNumber = decimal.Parse(record.Value);
					}
					catch (FormatException)
					{
						record.ValueAsNumber = (decimal)(double.Parse(record.Value));
					}
				}
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

		static SortedDictionary<TKey, List<TSource>> GroupBy<TSource, TKey>(IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector)
		{
			var res = source.GroupBy(keySelector).ToDictionary(pair => pair.Key, pair => pair.ToList());
			return new SortedDictionary<TKey, List<TSource>>(res);
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

		private static void WriteSummaryReport(ICollection<SessionEx> sessions, string resPath)
		{
			resPath = Path.GetFullPath(resPath + "\\Summary.txt");

			using (var file = new StreamWriter(resPath, false, Encoding.UTF8))
			{
				file.WriteLine("Name\tValue");

				var minDate = sessions.Min(session => session.LastUpdateTime);
				file.WriteLine("MinDate\t{0}", minDate.ToString("yyyy-MM-dd HH:mm:ss"));

				var maxDate = sessions.Max(session => session.LastUpdateTime);
				file.WriteLine("MaxDate\t{0}", maxDate.ToString("yyyy-MM-dd HH:mm:ss"));

				file.WriteLine("SessionsCount\t{0}", sessions.Count);

				var recordsCount = sessions.Aggregate(0, (val, session) => val + session.Records.Count);
				file.WriteLine("RecordsCount\t{0}", recordsCount);
			}
		}

		private static void WriteStatSummariesReport(IEnumerable<CalcResult> results, string resPath)
		{
			resPath = Path.GetFullPath(resPath + "\\LatencyStatSummaries.txt");

			using (var file = new StreamWriter(resPath, false, Encoding.UTF8))
			{
				file.WriteLine("Country\tCity\tLocation\tFunctionName\tCount\tAverage\tMin\tLowerQuartile\tMedian\tUpperQuartile\tMax");

				foreach (var result in results)
				{
					var summary = result.StatSummary;
					if (summary == null)
						continue;
					file.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}",
						result.Country, result.City, result.Location, result.FunctionName,
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
				file.WriteLine("Country\tCity\tLocation\tFunctionName\tLatency\tCount");

				foreach (var result in results)
				{
					if (result.Distribution == null)
						continue;
					foreach (var pair in result.Distribution.Vals)
					{
						file.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
						result.Country, result.City, result.Location, result.FunctionName,
						pair.Key, pair.Value);
					}
				}
			}
		}

		private List<SessionEx> _sessions;
	}
}
