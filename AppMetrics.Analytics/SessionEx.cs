using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AppMetrics.Shared;

namespace AppMetrics.Analytics
{
	public class SessionEx : Session
	{
		public string Ip;
		public Location Location { get; set; }

		public List<RecordEx> Records { get; set; }

		public override string ToString()
		{
			var locationText = "";
			if (Location != null)
				locationText = string.Format("'{0}' '{1}'", Location.countryName, Location.city);

			var res = string.Format("{0} {1} | {2} | {3}", Ip, locationText, Records.Count, LastUpdateTime);
			return res;
		}
	}
}
