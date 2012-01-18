using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AppMetrics.Analytics
{
	public class StatsBuilder
	{
		public List<CalcResult> Process(List<SessionEx> sessions)
		{
			_sessions = sessions;
			PrepareData();
			GC.Collect();

			var res = CalculateByCountries();
			return res;
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

			var sessionsByCountries = Util.GroupBy(_sessions, session => session.Location.countryName);
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

			var recordsByCities = Util.GroupBy(records, record => (record.Session.Location.city) ?? "");
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

			var recordsByFunction = Util.GroupBy(records, record => record.Name.Split(' ')[1]);
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

			var latencies = records.Where(Util.IsLatency).Select(record => record.ValueAsNumber).ToArray();
			if (latencies.Length > 0)
			{
				res.StatSummary = Stats.CalculateSummaries(latencies);
				res.Distribution = CalculateDistribution(latencies);
			}

			var jitterVals = records.Where(Util.IsJitter).Select(record => record.ValueAsNumber).ToList();
			if (jitterVals.Count > 0)
			{
				// remove highest 2% values
				jitterVals.Sort();
				var countToRemove = (int)(jitterVals.Count * 0.02);
				jitterVals.RemoveRange(jitterVals.Count - countToRemove, countToRemove);

				res.Jitter = CalculateJitterSummary(jitterVals.ToArray());
			}

			return res;
		}

		private static Distribution CalculateDistribution(decimal[] latencies)
		{
			var res = new Distribution { Count = latencies.Length };

			foreach (var latency in latencies)
			{
				var rounded = Util.Ceiling(latency, 0.5);
				if (res.Vals.ContainsKey(rounded))
					res.Vals[rounded]++;
				else
					res.Vals[rounded] = 1;
			}

			return res;
		}

		private static JitterSummary CalculateJitterSummary(decimal[] jitterVals)
		{
			var res = new JitterSummary { Count = jitterVals.Length };

			foreach (var jitter in jitterVals)
			{
				var rounded = Util.Ceiling(jitter, 0.2);
				if (res.Vals.ContainsKey(rounded))
					res.Vals[rounded]++;
				else
					res.Vals[rounded] = 1;
			}

			return res;
		}

		private void PrepareData()
		{
			var watch = Stopwatch.StartNew();

			var geoDataPath = Path.GetFullPath(@"..\..\tools\GeoIP\GeoLiteCity.dat");
			var geoLookup = new LookupService(geoDataPath, LookupService.GEOIP_MEMORY_CACHE);

			foreach (var session in _sessions)
			{
				var ipRecord = session.Records.Find(record => record.Name == "ClientIP");
				if (ipRecord == null)
				{
					session.Records.Clear();
					continue;
				}

				session.Ip = ipRecord.Value;
				session.Location = geoLookup.getLocation(session.Ip);

				session.Records.RemoveAll(record => !Util.IsLatency(record) && !Util.IsJitter(record));

				foreach (var record in session.Records)
				{
					decimal cur;
					if (!decimal.TryParse(record.Value, out cur))
						cur = (decimal)(double.Parse(record.Value));
					record.ValueAsNumber = cur;
				}

				AdjustJitter(session);
			}

			_sessions.RemoveAll(session => session.Records.Count == 0);

			Console.WriteLine("Preparing data: {0} secs", watch.Elapsed.TotalSeconds);
		}

		static void AdjustJitter(SessionEx session)
		{
			var jitterRecords = session.Records.Where(Util.IsJitter).ToArray();
			if (jitterRecords.Length == 0)
				return;

			var jitterRecordsSlicedByTime = Util.GroupBy(jitterRecords,
				record => Math.Floor((record.Time - DateTime.MinValue).TotalMinutes * 2)); // slice with period 30 secs

			foreach (var pair in jitterRecordsSlicedByTime)
			{
				var slicedRecords = pair.Value;
				var min = slicedRecords.Min(record => record.ValueAsNumber);

				foreach (var t in slicedRecords)
				{
					t.ValueAsNumber -= min;
				}
			}
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
