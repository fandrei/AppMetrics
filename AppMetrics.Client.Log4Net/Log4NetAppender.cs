using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net.Appender;
using log4net.Core;

namespace AppMetrics.Client.Log4Net
{
	public class Log4NetAppender : AppenderSkeleton
	{
		public string Server { get; set; }
		public string ApplicationKey { get; set; }
		public string AccessKey { get; set; }

		protected override void Append(LoggingEvent loggingEvent)
		{
			var appMetricsData = loggingEvent.MessageObject as AppMetricsMessage;
			if (appMetricsData != null)
				Tracker.Log(appMetricsData.Name, appMetricsData.Value);
			else
				Tracker.Log("Event", loggingEvent.MessageObject);
		}

		public TrackerBase Tracker
		{
			get
			{
				lock (_sync)
				{
					if (_tracker == null)
						_tracker = Client.Tracker.Create(Server, ApplicationKey, AccessKey);
					return _tracker;
				}
			}
		}

		readonly object _sync = new object();
		private Tracker _tracker;
	}
}
