using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace AppMetrics.Shared
{
	public class TimePeriod
	{
		private TimePeriod()
		{
		}

		public TimePeriod(DateTime start, DateTime end)
		{
			StartTime = start;
			EndTime = end;
		}

		public TimePeriod(NameValueCollection vals)
		{
			var startTimeString = vals.Get("StartTime") ?? "";
			if (!string.IsNullOrEmpty(startTimeString))
				StartTime = Util.ParseDateTime(startTimeString);

			var endTimeString = vals.Get("EndTime") ?? "";
			if (!string.IsNullOrEmpty(endTimeString))
				EndTime = Util.ParseDateTime(endTimeString);
		}

		public DateTime StartTime = DateTime.MinValue;
		public DateTime EndTime = DateTime.MaxValue;

		public static TimePeriod Unlimited { get { return new TimePeriod(); } }
	}
}