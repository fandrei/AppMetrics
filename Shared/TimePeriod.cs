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
			Read(vals);
		}

		private void Read(NameValueCollection vals)
		{
			var startTimeString = vals.Get("StartTime") ?? "";
			if (!string.IsNullOrEmpty(startTimeString))
				StartTime = Util.ParseDateTime(startTimeString);

			var endTimeString = vals.Get("EndTime") ?? "";
			if (!string.IsNullOrEmpty(endTimeString))
				EndTime = Util.ParseDateTime(endTimeString);
		}

		public static TimePeriod TryRead(NameValueCollection vals)
		{
			var res = new TimePeriod(vals);
			if (res.StartTime.Equals(Unlimited.StartTime))
				return null;
			return res;
		}

		public static TimePeriod Create(TimeSpan timeSpan)
		{
			var now = DateTime.UtcNow;
			var startTime = (timeSpan == TimeSpan.MaxValue) ? DateTime.MinValue : now - timeSpan;
			var res = new TimePeriod(startTime, now);
			return res;
		}

		public override string ToString()
		{
			var res = string.Format("'{0}'-'{1}'", Util.Format(StartTime), Util.Format(EndTime));
			return res;
		}

		public DateTime StartTime = DateTime.MinValue;
		public DateTime EndTime = DateTime.MaxValue;

		public static TimePeriod Unlimited { get { return new TimePeriod(); } }
	}
}