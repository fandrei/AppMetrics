using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AppMetrics.Analytics
{
	public static class IisLogReader
	{
		public static List<SessionEx> ReadIisData(string path)
		{
			var res = new List<SessionEx>();
			foreach (var file in Directory.GetFiles(path, "*.log"))
			{
				res.AddRange(ReadIisFile(file));
			}
			return res;
		}

		//[0]	"date"
		//[1]	"time"
		//[2]	"s-sitename"
		//[3]	"s-computername"
		//[4]	"s-ip"
		//[5]	"cs-method"
		//[6]	"cs-uri-stem"
		//[7]	"cs-uri-query"
		//[8]	"s-port"
		//[9]	"cs-username"
		//[10]	"c-ip"
		//[11]	"cs-version"
		//[12]	"cs(User-Agent)"
		//[13]	"cs(Cookie)"
		//[14]	"cs(Referer)"
		//[15]	"cs-host"
		//[16]	"sc-status"
		//[17]	"sc-substatus"
		//[18]	"sc-win32-status"
		//[19]	"sc-bytes"
		//[20]	"cs-bytes"
		//[21]	"time-taken"

		private static List<SessionEx> ReadIisFile(string path)
		{
			var text = File.ReadAllText(path);
			var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

			var sessions = new Dictionary<string, SessionEx>();

			foreach (var line in lines)
			{
				if (line.StartsWith("#"))
					continue;

				var parts = line.Split(' ');
				var dateTime = DateTime.Parse(parts[0] + " " + parts[1]);

				var status = parts[16];
				if (status != "200")
					continue;

				var url = parts[6].ToLowerInvariant();
				if (!url.StartsWith("/tradingapi/"))
					continue;

				var timeTaken = (double)int.Parse(parts[21]) / 1000;

				var ip = parts[10];
				SessionEx session;
				if (!sessions.TryGetValue(ip, out session))
				{
					session = new SessionEx { Records = new List<RecordEx>(), Ip = ip };
					session.Records.Add(new RecordEx(session) { Time = dateTime, Name = "ClientIP", Value = ip, });
					sessions.Add(ip, session);
				}

				var record = new RecordEx(session)
				{
					Time = dateTime,
					Name = "Latency " + url,
					Value = timeTaken.ToString(),
				};
				session.Records.Add(record);
			}

			return sessions.Values.ToList();
		}
	}
}
