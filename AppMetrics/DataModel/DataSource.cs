using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

			var curTime = DateTime.UtcNow;

			if (Directory.Exists(dataPath))
			{
				foreach (var filePath in Directory.GetFiles(dataPath, "*.*.txt", SearchOption.AllDirectories))
				{
					if (filePath.EndsWith(Const.LogFileName, StringComparison.OrdinalIgnoreCase))
						continue;

					var fileName = Path.GetFileNameWithoutExtension(filePath);
					var nameParts = fileName.Split('.');
					var sessionId = nameParts.Last();

					var timeText = nameParts.First().Replace('_', ':');
					var sessionCreationTime = DateTime.ParseExact(timeText, "u", CultureInfo.InvariantCulture);

					int timeZoneOffset;
					var lastUpdateTime = GetSessionLastWriteTime(filePath, sessionCreationTime, out timeZoneOffset);
					if (curTime - lastUpdateTime > period)
						continue;

					var session = new Session
									{
										FileName = filePath,
										Id = sessionId,
										CreationTime = sessionCreationTime,
										LastUpdateTime = lastUpdateTime,
										TimeZoneOffset = timeZoneOffset,
									};
					res.Add(session);
				}

				res.Sort((x, y) => x.CreationTime.CompareTo(y.CreationTime));
			}

			return res;
		}

		static DateTime GetSessionLastWriteTime(string filePath, DateTime creationUtcTime, out int timeZoneOffset)
		{
			// try to detect client's time zone from logged client time 
			// (can work incorrectly, if the first message was sent after delay > 1 hour)
			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					var firstLine = reader.ReadLine();
					var timeOffset = GetLineTime(firstLine) - creationUtcTime;
					timeZoneOffset = (int) Math.Round(timeOffset.TotalHours);

					var lastLine = ReadLastLine(reader);

					var res = GetLineTime(lastLine) - TimeSpan.FromHours(timeZoneOffset);
					return res;
				}
			}
		}

		private static DateTime GetLineTime(string line)
		{
			var tmp = line.Split('\t')[0];
			var res =DateTime.Parse(tmp);
			return res;
		}

		private static string ReadLastLine(StreamReader reader)
		{
			var stream = reader.BaseStream;
			var buf = new byte[1024 * 128];
			var seekPos =  Math.Min(buf.Length, stream.Length);
			stream.Seek(-seekPos, SeekOrigin.End);
			var lastBlockLength = stream.Read(buf, 0, buf.Length);

			int i = lastBlockLength - 2;
			for (; i >= 0; i--)
			{
				if (buf[i] == '\n')
				{
					i++;
					break;
				}
			}

			var lastLine = Encoding.UTF8.GetString(buf, i, lastBlockLength - i);
			return lastLine;
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
				var tmp = GetRecordsFromSession(session, period);
				res.AddRange(tmp);
			}

			return res;
		}

		public static List<Record> GetRecordsFromSession(Session session, TimeSpan period, bool filterRecords = true)
		{
			var curTime = DateTime.UtcNow;
			var res = new List<Record>();

			string text;
			using (var stream = new FileStream(session.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					text = reader.ReadToEnd();
				}
			}
			var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				var fields = line.Split('\t');

				var name = fields[1];
				var lineTime = GetLineTime(line);

				if (filterRecords && curTime - lineTime > period && !name.StartsWith("Client") && !name.StartsWith("System"))
					continue;

				var record = new Record
								{
									SessionId = session.Id,
									Time = lineTime,
									Name = name,
									Value = fields[2],
								};
				res.Add(record);
			}

			return res;
		}
	}
}