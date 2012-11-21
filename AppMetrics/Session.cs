using System;
using System.Collections.Generic;
using System.Linq;

using AppMetrics.Shared;

namespace AppMetrics
{
	public class Session
	{
		public string Id { get; set; }

		public DateTime CreationTime { get; set; }
		public DateTime LastUpdateTime { get; set; }

		public string FileName;

		public override string ToString()
		{
			var res = string.Format("{0} | {1}", CreationTime, LastUpdateTime);
			return res;
		}

		public string Serialize()
		{
			var res = string.Format("{0}\t{1}\t{2}", Id, Util.Serialize(CreationTime), Util.Serialize(LastUpdateTime));
			return res;
		}

		public static List<Session> Parse(string text)
		{
			var res = new List<Session>();

			var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				var columns = line.Split('\t');
				var cur = new Session
					{
						Id = columns[0],
						CreationTime = Util.ParseDateTime(columns[1]),
						LastUpdateTime = Util.ParseDateTime(columns[2]),
					};
				res.Add(cur);
			}

			return res;
		}
	}
}