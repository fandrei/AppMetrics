using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AppMetrics.DataModel;

namespace AppMetrics.Analytics
{
	class SessionEx : Session
	{
		public string Ip;
		public Location Location { get; set; }

		public List<RecordEx> Records { get; set; }

		public override string ToString()
		{
			var res = string.Format("{0} '{1}' '{2}' '{3}' {4}", Ip, Location.countryName, Location.regionName, 
				Location.city, Records.Count);
			return res;
		}
	}
}
