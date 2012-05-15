using System;
using System.Collections.Generic;
using System.Linq;

namespace AppMetrics.Shared
{
	public class Record
	{
		public string SessionId { get; set; }

		public DateTime Time { get; set; }
		public string Name { get; set; }
		public string Value { get; set; }

		public override string ToString()
		{
			var res = string.Format("{0} {1} {2}", Time, Name, Value);
			return res;
		}

		public string Serialize()
		{
			var res = string.Format("{0}\t{1}\t{2}\t{3}", SessionId, Time, Name, Value);
			return res;
		}
	}
}