using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;

namespace AppMetrics.Client.Log4Net
{
	public class Log4NetTracker : TrackerBase
	{
		public Log4NetTracker(ILog log)
		{
			_log = log;
		}

		public override void Dispose()
		{
		}

		public override void Log(string name, string val, MessagePriority priority = MessagePriority.Low)
		{
			var tmp = new AppMetricsMessage(name, val);
			_log.Info(tmp);
		}

		public override void FlushMessages()
		{
		}

		private readonly ILog _log;
	}
}
