using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using AppMetrics;

namespace DataImporter
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length != 5)
					throw new ApplicationException("Invalid command line arguments");
				var serverUrl = args[0];
				var sessionsUrl = serverUrl + "/GetSessions.ashx";
				var recordsUrl = serverUrl + "/GetRecords.ashx";

				var credentialsText = args[1];
				var parts = credentialsText.Split('|');
				if (parts.Length != 2)
					throw new ApplicationException("Invalid credentials");
				var userName = parts[0];
				var password = parts[1];

				var startTime = DateTime.Parse(args[2]);
				var appKey = args[3];

				var dataPath = args[4];

				using (var client = new WebClient())
				{
					client.Credentials = new NetworkCredential(userName, password);
					client.QueryString["AppKey"] = appKey;
					client.QueryString["StartTime"] = startTime.ToString("u");

					var sessionsResponse = client.DownloadString(sessionsUrl);
					var sessions = Session.Parse(sessionsResponse);

					foreach (var session in sessions)
					{
						client.QueryString.Remove("StartTime");
						client.QueryString["SessionId"] = session.Id;

						var recordsResponse = client.DownloadString(recordsUrl);

						var res = ConvertData(recordsResponse, session);
						var filePath = Const.FormatFilePath(dataPath, session.Id, session.CreationTime);
						File.WriteAllText(filePath, res);
					}
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}

		private static string ConvertData(string recordsResponse, Session session)
		{
			var buf = new StringBuilder();
			var lines = recordsResponse.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				if (!line.StartsWith(session.Id))
					throw new ApplicationException();
				var tmp = line.Substring(session.Id.Length + 1);
				buf.AppendLine(tmp);
			}
			var res = buf.ToString();
			return res;
		}
	}
}
