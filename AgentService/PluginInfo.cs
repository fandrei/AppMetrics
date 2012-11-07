using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.AgentService
{
	class PluginInfo
	{
		public PluginInfo(string name)
		{
			Name = name;
		}

		public string Name;

		public override string ToString()
		{
			return Name;
		}
	}
}
