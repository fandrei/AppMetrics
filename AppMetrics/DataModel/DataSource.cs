using System;
using System.Collections.Generic;
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
				RefreshData();
				return _sessions.AsQueryable();
			}
		}

		public IQueryable<Record> Records
		{
			get
			{
				RefreshData();
				return _records.AsQueryable();
			}
		}

		void RefreshData()
		{
			_sessions.Clear();
			_records.Clear();

			var dataPath = Util.GetDataFolderPath();
			foreach (var file in Directory.GetFiles(dataPath, "*.*.txt", SearchOption.AllDirectories))
			{
				if (file.EndsWith(Const.LogFileName, StringComparison.OrdinalIgnoreCase))
					continue;

				var name = file.Substring(dataPath.Length + 1);
				var fileTime = File.GetLastWriteTime(file);
				var item = new Session
				{
					Id = name,
					LastUpdated = fileTime,
					Records = new List<Record>(),
				};
				_sessions.Add(item);

				var text = File.ReadAllText(file);
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
					item.Records.Add(record);

					_records.Add(record);
				}
			}
			_sessions.Sort((x, y) => x.LastUpdated.CompareTo(y.LastUpdated));
		}

		private readonly List<Session> _sessions = new List<Session>();
		private readonly List<Record> _records = new List<Record>();
	}
}