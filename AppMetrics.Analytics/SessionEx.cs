using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AppMetrics.DataModel;

namespace AppMetrics.Analytics
{
	public class SessionEx : Session
	{
		public string Ip;
		public Location Location { get; set; }

		public List<RecordEx> Records { get; set; }

		public override string ToString()
		{
			var res = string.Format("{0} '{1}' '{2}' {3}|{4}", Ip, Location.countryName, 
				Location.city, Records.Count, LastUpdateTime);
			return res;
		}
	}
}
