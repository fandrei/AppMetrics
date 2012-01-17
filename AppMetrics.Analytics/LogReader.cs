using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using AppMetrics.DataModel;

namespace AppMetrics.Analytics
{
	public static class LogReader
	{
		public static List<SessionEx> Parse(string dataPath, TimeSpan period)
		{
			var watch = Stopwatch.StartNew();

			var res = new List<SessionEx>();

			{
				var sessions = DataSource.GetSessionsFromPath(dataPath, period);
				foreach (var session in sessions)
				{
					var records = DataSource.GetRecordsFromSession(session);

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