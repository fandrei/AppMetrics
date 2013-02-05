using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AppMetrics.AgentService.PluginBase
{
	public class Control
	{
		public static bool IsMasterRunning
		{
			get
			{
				var processFile = Process.GetCurrentProcess().MainModule.FileName.ToLower();
				if (processFile.EndsWith(".vshost.exe"))
					return true; // always true when running from VS

				var masterProcesses = Process.GetProcessesByName("AppMetrics.AgentService");
				if (masterProcesses.Length > 0)
					return true;

				masterProcesses = Process.GetProcessesByName("AppMetrics.AgentService.vshost");
				return (masterProcesses.Length > 0);
			}
		}
	}
}
