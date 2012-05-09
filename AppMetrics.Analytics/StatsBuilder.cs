using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AppMetrics.Analytics
{
	public class StatsBuilder
	{
		public List<CalcResult> Process(List<SessionEx> sessions, AnalysisOptions options)
		{
			if (options == null)
				options = new AnalysisOptions();
			options.Validate();

			_options = options;
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

			if (_options.LocationIncludeOverall || _options.SliceByLocation == LocationSliceType.None)
			{
				var allRecords = GetRecords(_sessions);
				var overallSummariesByFunction = CalculateByFunction(allRecords);
				foreach (var summary in overallSummariesByFunction)
				{
					summary.Country = "(World)";
				}
				res.AddRange(overallSummariesByFunction);
			}

			if (_options.SliceByLocation != LocationSliceType.None)
			{
				var sessionsFilteredByLocation = _sessions.Where(session => _options.LocationIsAllowed(session.Location)).ToArray();
				var sessionsByCountry = Util.GroupBySorted(sessionsFilteredByLocation, session => session.Location.countryName);

				foreach (var pair in sessionsByCountry)
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
			}

			Console.WriteLine("Finding statistic summaries: {0} secs", watch.Elapsed.TotalSeconds);
			watch.Stop();

			return res;
		}

		private List<CalcResult> CalculateByCities(ICollection<RecordEx> records)
		{
			var res = new List<CalcResult>();

			if (_options.LocationIncludeOverall || _options.SliceByLocation == LocationSliceType.Countries)
			{
				var tmp = CalculateByFunction(records);
				res.AddRange(tmp);
				foreach (var summary in tmp)
				{
					summary.City = "(All)";
				}
			}

			if (_options.SliceByLocation == LocationSliceType.CountriesAndCities)
			{
				var recordsByCity = Util.GroupBySorted(records, record => (record.Session.Location.city) ?? "");
				recordsByCity.Remove("");

				if (recordsByCity.Count > 1 || !_options.LocationIncludeOverall)
				{
					foreach (var pair in recordsByCity)
					{
						var cityName = pair.Key;
						if (string.IsNullOrEmpty(cityName))
							continue;

						var regionName = pair.Value.First().Session.Location.regionName;

						var curSummaries = CalculateByFunction(pair.Value);
						foreach (var summary in curSummaries)
						{
							summary.City = cityName;
							summary.Region = regionName;
						}
						res.AddRange(curSummaries);
					}
				}
			}

			return res;
		}

		private List<CalcResult> CalculateByFunction(ICollection<RecordEx> records)
		{
			var res = new List<CalcResult>();

			if (!_options.SliceByFunction)
			{
				var curSummary = Calculate(records);
				res.Add(curSummary);
			}
			else
			{
				records = records.Where(val => !Util.IsException(val)).ToArray();

				var recordsByFunction = Util.GroupBySorted(records, record => record.Name.Split(' ')[1]);
				foreach (var pair in recordsByFunction)
				{
					var functionName = pair.Key;
					if (string.IsNullOrEmpty(functionName))
						throw new ApplicationException();

					var curSummary = Calculate(pair.Value);
					curSummary.FunctionName = functionName;
					res.Add(curSummary);
				}
			}

			return res;
		}

		private static CalcResult Calculate(ICollection<RecordEx> records)
		{
			var res = new CalcResult();

			var exceptionRecords = records.Where(Util.IsException).ToArray();
			var exceptionsGrouped = Util.GroupBy(exceptionRecords, record => record.Value);
			foreach (var pair in exceptionsGrouped)
			{
				res.Exceptions.Add(pair.Key, pair.Value.Count);
			}
			res.ExceptionsCount = exceptionRecords.Length;

			var latencyRecords = records.Where(val => Util.IsLatency(val) && !Util.IsStreaming(val)).ToArray();
			if (latencyRecords.Length > 0)
			{
				var latencies = latencyRecords.Select(record => record.ValueAsNumber).ToList();
				latencies.Sort();

				res.StatSummary = Stats.CalculateSummaries(latencies);
				res.LatencyDistribution = Stats.CalculateDistribution(latencies.ToArray(), 0.5M);
			}

			var streamingLatencyRecords = GetLatencyRecords(records);
			if (streamingLatencyRecords.Length > 0)
			{
				var latencies = streamingLatencyRecords.Select(record => record.ValueAsNumber).ToList();
				latencies.Sort();

				res.StreamingStatSummary = Stats.CalculateSummaries(latencies);
				res.StreamingLatencyDistribution = Stats.CalculateDistribution(latencies.ToArray(), 0.5M);
			}

			res.StreamingJitter = CalculateJitter(records);

			return res;
		}

		private static Distribution CalculateJitter(ICollection<RecordEx> records)
		{
			var streamingRecords = records.Where(val => (Util.IsJitter(val) || Util.IsLatency(val)) &&
				Util.IsStreaming(val)).ToArray();

			var jitterVals = new List<decimal>();

			var recordsBySessions = Util.GroupBy(streamingRecords, record => record.Session.Id);
			foreach (var pair in recordsBySessions)
			{
				var cur = GetJitterVals(pair.Value);
				jitterVals.AddRange(cur);
			}

			if (jitterVals.Count > 0)
			{
				return Stats.CalculateDistribution(jitterVals.ToArray(), 0.2M);
			}
			return null;
		}

		static RecordEx[] GetLatencyRecords(ICollection<RecordEx> records)
		{
			var res = new List<RecordEx>();

			var filteredRecords = records.Where(val => IsNtpdInfo(val) || (Util.IsLatency(val) && Util.IsStreaming(val))).ToArray();
			var recordsBySessions = Util.GroupBy(filteredRecords, record => record.Session.Id);

			foreach (var pair in recordsBySessions)
			{
				var sessionRecords = pair.Value;
				if (sessionRecords.Count == 0)
					continue;

				var timeIsStable = false;
				foreach (var record in sessionRecords)
				{
					if (IsNtpdInfo(record))
					{
						var jitter = GetNtpdJitter(record);
						timeIsStable = (jitter < MaxTimeJitter);
					}
					else
					{
						if (timeIsStable)
						{
							res.Add(record);
						}
					}
				}
			}

			return res.ToArray();
		}

		private const double MaxTimeJitter = 0.01;

		private static double GetNtpdJitter(RecordEx record)
		{
			var text = record.Value;
			var infoText = text.Split(':')[1];
			var parts = infoText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			var minParts = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (minParts[0] != "min")
				throw new ApplicationException();

			var maxParts = parts[2].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (maxParts[0] != "max")
				throw new ApplicationException();

			var min = double.Parse(minParts[1]);
			var max = double.Parse(maxParts[1]);

			var res = Math.Max(Math.Abs(max), Math.Abs(min));
			return res;
		}

		static bool IsNtpdInfo(RecordEx record)
		{
			return Util.IsInfo(record) && record.Value.StartsWith("ntpd");
		}

		private void PrepareData()
		{
			var watch = Stopwatch.StartNew();

			var geoLookup = new LookupService(LookupService.GEOIP_MEMORY_CACHE);
			var overrides = GetLocationOverrides();

			foreach (var session in _sessions)
			{
				var ipRecord = session.Records.Find(record => record.Name == "ClientIP");
				if (ipRecord == null)
				{
					session.Records.Clear();
					continue;
				}

				var ip = ipRecord.Value;
				session.Ip = ip;
				session.Location = overrides.ContainsKey(ip) ? overrides[ip] : geoLookup.getLocation(ip);

				session.Records.RemoveAll(record => !Util.IsLatency(record) && !Util.IsJitter(record) &&
					!Util.IsException(record) && !Util.IsInfo(record));

				foreach (var record in session.Records)
				{
					if (!Util.IsLatency(record) && !Util.IsJitter(record))
						continue;
					decimal cur;
					if (!decimal.TryParse(record.Value, out cur))
						cur = (decimal)(double.Parse(record.Value));
					record.ValueAsNumber = cur;
				}

				Validate(session);
			}

			_sessions.RemoveAll(session => session.Records.Count == 0);

			Console.WriteLine("Preparing data: {0} secs", watch.Elapsed.TotalSeconds);
		}

		static Dictionary<string, Location> GetLocationOverrides()
		{
			var path = AppMetrics.Util.GetAppLocation() + @"\GeoIP\Override.txt";
			var text = System.IO.File.ReadAllText(path);
			var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			var res = new Dictionary<string, Location>();
			foreach (var line in lines)
			{
				var parts = line.Split('\t');
				var ip = parts[0];
				var loc = new Location { countryName = parts[1], regionName = parts[2], city = parts[3] };
				res.Add(ip, loc);
			}
			return res;
		}

		static decimal[] GetJitterVals(IList<RecordEx> jitterRecords)
		{
			if (jitterRecords.Count == 0)
				return new decimal[0];

			var res = new List<decimal>();

			// slice records with period of 30 secs
			var jitterRecordsSlicedByTime = Util.GroupBySorted(jitterRecords,
				record => Math.Floor((record.Time - DateTime.MinValue).TotalMinutes * 2));

			foreach (var pair in jitterRecordsSlicedByTime)
			{
				var slicedRecords = pair.Value;
				var min = slicedRecords.Min(record => record.ValueAsNumber);

				foreach (var t in slicedRecords)
				{
					res.Add(t.ValueAsNumber - min);
				}
			}

			return res.ToArray();
		}

		static void Validate(SessionEx session)
		{
			if (session.Records.Any(record => Util.IsJitter(record) && record.ValueAsNumber < 0))
				throw new ValidationException();
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
		private AnalysisOptions _options;
	}
}
