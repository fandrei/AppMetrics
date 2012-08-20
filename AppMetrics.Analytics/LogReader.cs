using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using AppMetrics.Shared;
using AppMetrics.WebUtils;

namespace AppMetrics.Analytics
{
	public static class LogReader
	{
		public static List<SessionEx> Parse(AnalysisOptions options)
		{
			var dataPath = SiteConfig.DataStoragePath + "\\" + options.ApplicationKey;
			return Parse(dataPath, options.Period);
		}

		public static List<SessionEx> Parse(string dataPath, TimeSpan timeSpan)
		{
			var watch = Stopwatch.StartNew();

			var res = new List<SessionEx>();

			var startTime = (timeSpan == TimeSpan.MaxValue) ? DateTime.MinValue : DateTime.UtcNow - timeSpan;
			var period = new TimePeriod(startTime, DateTime.MaxValue);
			{
				var sessions = DataReader.GetSessionsFromPath(dataPath, period);

				foreach (var session in sessions)
				{
					var records = DataReader.GetRecordsFromSession(session, period);

					var sessionEx = new SessionEx
										{
											Id = session.Id,
											CreationTime = session.CreationTime,
											LastUpdateTime = session.LastUpdateTime
										};
					res.Add(sessionEx);

					sessionEx.Records = records.ConvertAll(
						val => new RecordEx(sessionEx)
								{
									SessionId = val.SessionId,
									Name = val.Name,
									Time = val.Time,
									Value = val.Value
								});
				}
			}

			GC.Collect();
			Trace.WriteLine(string.Format("Parsing data: {0} secs", watch.Elapsed.TotalSeconds));
			return res;
		}
	}
}