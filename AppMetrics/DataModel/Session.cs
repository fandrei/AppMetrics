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

		public double TimeZoneHours
		{
			get { return TimeZoneOffset.TotalHours; }
			set { TimeZoneOffset = TimeSpan.FromHours(value); }
		}

		public TimeSpan TimeZoneOffset;

		public string FileName;

		public override string ToString()
		{
			var res = string.Format("{0} | {1} | {2}", CreationTime, LastUpdateTime, TimeZoneOffset);
			return res;
		}
	}
}