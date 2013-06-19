using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.Client.Log4Net
{
	class AppMetricsMessage
	{
		public AppMetricsMessage(string name, string value)
		{
			Name = name;
			Value = value;
		}

		public string Name;
		public string Value;

		public override string ToString()
		{
			return string.Format("{0}: {1}", Name, Value); ;
		}
	}
}
