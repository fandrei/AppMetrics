using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using AppMetrics.Shared;
using AppMetrics.WebUtils;

namespace AppMetrics
{
	public static class DataReader
	{
		public static List<Session> GetSessions(string appKey, TimePeriod period)
		{
			var dataPath = Path.Combine(SiteConfig.DataStoragePath, appKey);
			return GetSessionsFromPath(dataPath, period);
		}

		public static List<Session> GetSessionsFromPath(string dataPath, TimePeriod period)
		{
			var res = new List<Session>();

			if (Directory.Exists(dataPath))
			{
				foreach (var filePath in Directory.GetFiles(dataPath, "*.*.txt", SearchOption.AllDirectories))
				{
					if (filePath.EndsWith(WebLogger.FileName, StringComparison.OrdinalIgnoreCase))
						continue;

					var session = ReadSession(filePath, period);
					if (session != null)
						res.Add(session);
				}

				res.Sort((x, y) => x.CreationTime.CompareTo(y.CreationTime));
			}

			return res;
		}

		public static Session ReadSession(string filePath, TimePeriod period)
		{
			try
			{
				var fileName = Path.GetFileNameWithoutExtension(filePath);
				var nameParts = fileName.Split('.');
				var sessionId = nameParts.Last();

				var timeText = nameParts.First().Replace('_', ':');
				var sessionCreationTime = Util.ParseDateTime(timeText);
				if (sessionCreationTime > period.EndTime)
					return null;

				var lastUpdateTime = GetSessionLastWriteTime(sessionId, filePath);
				if (lastUpdateTime < period.StartTime)
					return null;

				return new Session
					{
						FileName = filePath,
						Id = sessionId,
						CreationTime = sessionCreationTime,
						LastUpdateTime = lastUpdateTime,
					};
			}
			catch (Exception exc)
			{
				var message = string.Format("Error in session file {0}", filePath);
				throw new ApplicationException(message, exc);
			}
		}

		public static Session ReadSession(string appKey, string sessionId, TimePeriod period)
		{
			var mask = string.Format("*.{0}.txt", sessionId);
			var fileList = Directory.GetFiles(GetSessionsDataPath(appKey), mask, SearchOption.AllDirectories);

			if (fileList.Length == 0)
				return null;
			if (fileList.Length != 1)
				throw new ApplicationException();

			var res = ReadSession(fileList[0], period);
			return res;
		}

		public static DateTime GetSessionLastWriteTime(string sessionId, string filePath)
		{
			using (var mutex = Utils.TryLockFile(sessionId, filePath))
			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				var encoding = DetectEncoding(stream);

				var lastLine = ReadLastLine(stream, encoding);
				var res = GetLineTime(lastLine);
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

		static string ReadLine(Stream stream, Encoding encoding)
		{
			var buffer = new List<byte>(8 * 1024);
			while (true)
			{
				var cur = stream.ReadByte();
				if (cur < 0)
					break;

				if (cur == '\r')
					continue;
				if (cur == '\n')
					break;

				buffer.Add((byte)cur);
			}
			if (buffer.Count == 0)
				return null;
			var res = encoding.GetString(buffer.ToArray());
			return res;
		}

		private static DateTime GetLineTime(string line)
		{
			var text = line.Split('\t')[0];
			return Util.ParseDateTime(text);
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

		public static List<Record> GetRecords(string appKey, TimePeriod period)
		{
			var dataPath = GetSessionsDataPath(appKey);
			return GetRecordsFromPath(dataPath, period);
		}

		public static List<Record> GetRecordsFromPath(string dataPath, TimePeriod period)
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

		public static List<Record> GetRecordsFromSession(Session session, TimePeriod period, bool filterRecords = true)
		{
			var res = new List<Record>();

			using (var mutex = Utils.TryLockFile(session.Id, session.FileName))
			using (var stream = new FileStream(session.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				var encoding = DetectEncoding(stream);

				if (filterRecords)
				{
					while (true)
					{
						var startPos = stream.Position;
						var line = ReadLine(stream, encoding);
						if (line == null)
							break;

						var record = ParseLine(line);
						if (!IsServiceMessage(record.Name))
						{
							stream.Position = startPos;
							break;
						}

						record.SessionId = session.Id;
						res.Add(record);
					}

					SkipOutdatedRecords(stream, encoding, period.StartTime);
				}

				using (var reader = new StreamReader(stream, encoding, true))
				{
					while (true)
					{
						var line = reader.ReadLine();
						if (line == null)
							break;

						var record = ParseLine(line);

						if (filterRecords && record.Time > period.EndTime)
							break;
						if (filterRecords && record.Time < period.StartTime)
							continue;

						record.SessionId = session.Id;
						res.Add(record);
					}
				}
			}

			return res;
		}

		private static void SkipOutdatedRecords(Stream stream, Encoding encoding, DateTime startTime)
		{
			if (stream.Length - stream.Position < 16 * 1024)
				return;

			var pos = stream.Position;

			while (true)
			{
				ReadLine(stream, encoding); // skip line - it can be incomplete
				var line = ReadLine(stream, encoding);
				if (line == null)
					break;

				var lineTime = GetLineTime(line);
				if (lineTime > startTime)
					break;

				pos = stream.Position;
				var newPos = stream.Position + (stream.Length - stream.Position) / 2;
				stream.Position = newPos;
			}

			stream.Position = pos;
		}

		private static Record ParseLine(string line)
		{
			var fields = line.Split('\t');

			var name = fields[1];
			var lineTime = GetLineTime(line);

			return new Record
					{
						Time = lineTime,
						Name = name,
						Value = fields[2],
					};
		}

		static bool IsServiceMessage(string name)
		{
			return name.StartsWith("Client") || name.StartsWith("System") || name.StartsWith("Info");
		}

		private static string GetSessionsDataPath(string appKey)
		{
			return Path.Combine(SiteConfig.DataStoragePath, appKey);
		}
	}
}