using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		public string Name { get; private set; }
		public Process Process { get; set; }

		public override string ToString()
		{
			return Name;
		}

		public bool IsStarted
		{
			get
			{
				if (Process == null)
					return false;
				var isStopped = Process.WaitForExit(0);
				if (isStopped)
					Process = null;
				return !isStopped;
			}
		}
	}
}
