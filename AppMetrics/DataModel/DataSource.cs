using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace AppMetrics.DataModel
{
	public partial class DataSource
	{
		public IQueryable<Session> Sessions
		{
			get
			{
				string appKey;
				TimeSpan period;
				GetParams(out appKey, out period);

				var res = GetSessions(appKey, period);

				return res.AsQueryable();
			}
		}

		public IQueryable<Record> Records
		{
			get
			{
				string appKey;
				TimeSpan period;
				GetParams(out appKey, out period);

				var res = GetRecords(appKey, period);

				return res.AsQueryable();
			}
		}

		static void GetParams(out string appKey, out TimeSpan period)
		{
			var args = HttpContext.Current.Request.Params;

			appKey = args["appKey"];

			var periodText = args["period"];
			period = TimeSpan.Parse(periodText);
		}

		public static List<Session> GetSessions(string appKey, TimeSpan period)
		{
			var dataPath = Path.Combine(AppSettings.DataStoragePath, appKey);
			return GetSessionsFromPath(dataPath, period);
		}

		public static List<Session> GetSessionsFromPath(string dataPath, TimeSpan period)
		{
			var res = new List<Session>();

			var beginningTime = DateTime.Now - period;

			if (Directory.Exists(dataPath))
			{
				foreach (var filePath in Directory.GetFiles(dataPath, "*.*.txt", SearchOption.AllDirectories))
				{
					if (filePath.EndsWith(Const.LogFileName, StringComparison.OrdinalIgnoreCase))
						continue;

					var fileName = Path.GetFileNameWithoutExtension(filePath);
					var nameParts = fileName.Split('.');
					var sessionId = nameParts.Last();

					var creationTime = File.GetCreationTime(filePath);
					if (creationTime < beginningTime)
						continue;

					var lastUpdateTime = File.GetLastWriteTime(filePath);

					var session = new Session
									{
										FileName = filePath,
										Id = sessionId,
										CreationTime = creationTime,
										LastUpdateTime = lastUpdateTime,
									};
					res.Add(session);
				}

				res.Sort((x, y) => x.LastUpdateTime.CompareTo(y.LastUpdateTime));
			}

			return res;
		}

		public static List<Record> GetRecords(string appKey, TimeSpan period)
		{
			var dataPath = Path.Combine(AppSettings.DataStoragePath, appKey);
			return GetRecordsFromPath(dataPath, period);
		}

		public static List<Record> GetRecordsFromPath(string dataPath, TimeSpan period)
		{
			var res = new List<Record>();

			var sessions = GetSessionsFromPath(dataPath, period);

			foreach (var session in sessions)
			{
				var tmp = GetRecordsFromSession(session);
				res.AddRange(tmp);
			}

			return res;
		}

		public static List<Record> GetRecordsFromSession(Session session)
		{
			var res = new List<Record>();

			var text = File.ReadAllText(session.FileName);
			var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				var fields = line.Split('\t');

				var record = new Record
								{
									SessionId = session.Id,
									Time = DateTime.Parse(fields[0]),
									Name = fields[1],
									Value = fields[2],
								};
				res.Add(record);
			}

			return res;
		}
	}
}