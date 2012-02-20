using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;

namespace AppMetrics.DataModel
{
	[DataServiceKey("Id")]
	public class Session
	{
		public string Id { get; set; }

		public DateTime CreationTime { get; set; }
		public DateTime LastUpdateTime { get; set; }
		public int TimeZoneOffset { get; set; }

		public string FileName { get; internal set; }

		public override string ToString()
		{
			var res = string.Format("{0} | {1} | {2}", CreationTime, LastUpdateTime, TimeZoneOffset);
			return res;
		}
	}
}