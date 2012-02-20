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
					var sessionCreationTime = ParseDateTime(timeText);

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
				var encoding = DetectEncoding(stream);

				var firstLine = ReadLine(stream, encoding);
				var timeOffset = GetLineTime(firstLine) - creationUtcTime;
				timeZoneOffset = (int)Math.Round(timeOffset.TotalHours);

				var lastLine = ReadLastLine(stream, encoding);
				if (lastLine == null)
					lastLine = firstLine;

				var res = GetLineTime(lastLine) - TimeSpan.FromHours(timeZoneOffset);
				return res;
			}
		}

		static Encoding DetectEncoding(Stream stream)
		{
			foreach (var cur in Const.Utf8Bom)
			{
				if (stream.ReadByte() != cur)
				{
					stream.Position = 0;
					break;
				}
			}
			return Encoding.UTF8;
		}

		private static readonly List<byte> _buf = new List<byte>(1024 * 1024);

		static string ReadLine(Stream stream, Encoding encoding)
		{
			_buf.Clear();
			while (true)
			{
				var cur = stream.ReadByte();
				if (cur < 0)
					break;

				_buf.Add((byte)cur);

				if (cur == '\n')
					break;
			}
			if (_buf.Count == 0)
				return null;
			var res = encoding.GetString(_buf.ToArray());
			return res;
		}

		private static DateTime GetLineTime(string line)
		{
			var text = line.Split('\t')[0];
			return ParseDateTime(text);
		}

		private static DateTime ParseDateTime(string text)
		{
			var formats = new[]
				{
					"yyyy-MM-dd HH:mm:ss.fffffff",
					"yyyy-MM-dd HH:mm:ss",
					"u",
				};
			DateTime res;
			if (!DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out res))
				throw new ArgumentException();
			return res;
		}

		private static string ReadLastLine(Stream stream, Encoding encoding)
		{
			var buf = new byte[1024 * 128];
			var seekPos = Math.Min(buf.Length, stream.Length - stream.Position);
			stream.Seek(-seekPos, SeekOrigin.End);
			var lastBlockLength = stream.Read(buf, 0, buf.Length);

			int i = lastBlockLength - 2;
			if (i < 0)
				return null;

			for (; i > 0; i--)
			{
				if (buf[i] == '\n')
				{
					i++;
					break;
				}
			}

			var lastLine = encoding.GetString(buf, i, lastBlockLength - i);
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

			using (var stream = new FileStream(session.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8, true))
				{
					while (true)
					{
						var line = reader.ReadLine();
						if (line == null)
							break;

						var record = ParseLine(line, session.Id);
						if (filterRecords && !record.Name.StartsWith("Client") && !record.Name.StartsWith("System"))
						{
							if (curTime - record.Time > period)
								continue;
						}
						res.Add(record);
					}
				}
			}

			return res;
		}

		private static Record ParseLine(string line, string sessionId)
		{
			var fields = line.Split('\t');

			var name = fields[1];
			var lineTime = GetLineTime(line);

			return new Record
					{
						SessionId = sessionId,
						Time = lineTime,
						Name = name,
						Value = fields[2],
					};
		}
	}
}