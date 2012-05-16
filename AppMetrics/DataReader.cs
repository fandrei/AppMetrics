using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using AppMetrics.Shared;

namespace AppMetrics
{
	public static class DataReader
	{
		public static List<Session> GetSessions(string appKey, DateTime startTime)
		{
			var dataPath = Path.Combine(SiteConfig.DataStoragePath, appKey);
			return GetSessionsFromPath(dataPath, startTime);
		}

		public static List<Session> GetSessionsFromPath(string dataPath, DateTime startTime)
		{
			var res = new List<Session>();

			if (Directory.Exists(dataPath))
			{
				foreach (var filePath in Directory.GetFiles(dataPath, "*.*.txt", SearchOption.AllDirectories))
				{
					if (filePath.EndsWith(Const.LogFileName, StringComparison.OrdinalIgnoreCase))
						continue;

					var session = ReadSession(filePath, startTime);
					if (session != null)
						res.Add(session);
				}

				res.Sort((x, y) => x.CreationTime.CompareTo(y.CreationTime));
			}

			return res;
		}

		public static Session ReadSession(string filePath, DateTime startTime)
		{
			var fileName = Path.GetFileNameWithoutExtension(filePath);
			var nameParts = fileName.Split('.');
			var sessionId = nameParts.Last();

			var timeText = nameParts.First().Replace('_', ':');
			var sessionCreationTime = Util.ParseDateTime(timeText);

			var lastUpdateTime = GetSessionLastWriteTime(filePath);
			if (lastUpdateTime < startTime)
				return null;

			return new Session
				{
					FileName = filePath,
					Id = sessionId,
					CreationTime = sessionCreationTime,
					LastUpdateTime = lastUpdateTime,
				};
		}

		public static Session ReadSession(string appKey, string sessionId, DateTime startTime)
		{
			throw new NotImplementedException();
		}

		static DateTime GetSessionLastWriteTime(string filePath)
		{
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

		private static readonly List<byte> _buf = new List<byte>(1024 * 1024);

		static string ReadLine(Stream stream, Encoding encoding)
		{
			_buf.Clear();
			while (true)
			{
				var cur = stream.ReadByte();
				if (cur < 0)
					break;

				if (cur == '\r')
					continue;
				if (cur == '\n')
					break;

				_buf.Add((byte)cur);
			}
			if (_buf.Count == 0)
				return null;
			var res = encoding.GetString(_buf.ToArray());
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

		public static List<Record> GetRecords(string appKey, DateTime startTime)		{			var dataPath = Path.Combine(SiteConfig.DataStoragePath, appKey);			return GetRecordsFromPath(dataPath, startTime);		}
		public static List<Record> GetRecordsFromPath(string dataPath, DateTime startTime)
		{
			var res = new List<Record>();

			var sessions = GetSessionsFromPath(dataPath, startTime);

			foreach (var session in sessions)
			{
				var tmp = GetRecordsFromSession(session, startTime);
				res.AddRange(tmp);
			}

			return res;
		}

		public static List<Record> GetRecordsFromSession(Session session, DateTime startTime, bool filterRecords = true)
		{
			var res = new List<Record>();

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

					SkipOutdatedRecords(stream, encoding, startTime);
				}

				using (var reader = new StreamReader(stream, encoding, true))
				{
					while (true)
					{
						var line = reader.ReadLine();
						if (line == null)
							break;

						var record = ParseLine(line);
						if (filterRecords && record.Time < startTime)
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
			return name.StartsWith("Client") || name.StartsWith("System");
		}
	}
}