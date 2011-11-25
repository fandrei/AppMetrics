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
			var res = new List<Session>();
			
			var beginningTime = DateTime.Now - period;
			var dataPath = Path.Combine(Util.GetDataFolderPath(), appKey);
			foreach (var file in Directory.GetFiles(dataPath, "*.*.txt", SearchOption.AllDirectories))
			{
				if (file.EndsWith(Const.LogFileName, StringComparison.OrdinalIgnoreCase))
					continue;

				var fileTime = File.GetCreationTime(file);
				if (fileTime < beginningTime)
					continue;

				var name = file.Substring(dataPath.Length + 1);
				var session = new Session
				{
					FileName = file,
					Id = name,
					LastUpdated = fileTime,
				};
				res.Add(session);
			}

			res.Sort((x, y) => x.LastUpdated.CompareTo(y.LastUpdated));

			return res;
		}

		public static List<Record> GetRecords(string appKey, TimeSpan period)
		{
			var res = new List<Record>();

			var sessions = GetSessions(appKey, period);
			foreach (var session in sessions)
			{
				var text = File.ReadAllText(session.FileName);
				var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

				foreach (var line in lines)
				{
					var fields = line.Split('\t');

					var record = new Record
					{
						Time = DateTime.Parse(fields[0]),
						Name = fields[1],
						Value = fields[2],
					};
					res.Add(record);
				}
			}

			return res;
		}
	}
}