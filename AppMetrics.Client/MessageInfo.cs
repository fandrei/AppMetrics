using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.Client
{
	class MessageInfo
	{
		public string Name;
		public string Value;
		public string SessionId;
		public DateTime Time;
		public MessageSeverity Severity;

		public override string ToString()
		{
			var res = string.Format("[{0} {1}] {2}: {3}", Time, Severity, Name, Value);
			return res;
		}
	}

	public enum MessageSeverity { Low = 0, High }
}
