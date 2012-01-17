using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AppMetrics.DataModel;

namespace AppMetrics.Analytics
{
	public class RecordEx : Record
	{
		public RecordEx(SessionEx session)
		{
			Session = session;
		}

		public SessionEx Session { get; private set; }

		public decimal ValueAsNumber { get; set; }
	}
}
