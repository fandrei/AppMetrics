using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.AgentService
{
	public static class Const
	{
		public const string AppName = "AppMetrics Agent";

		public static string WorkingAreaPath
		{
			get
			{
				var basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				var res = basePath + @"\City Index\AppMetrics Agent\";
				return res;
			}
		}

		public static string GetPluginBinPath(string name)
		{
			return WorkingAreaBinPath + @"\" + name + @"\";
		}

		public static string GetPluginExePath(string name)
		{
			return GetPluginBinPath(name) + @"\AppMetrics_" + name + ".exe";
		}

		public static string WorkingAreaBinPath
		{
			get { return WorkingAreaPath + @"\bin\"; }
		}

		public static string WorkingAreaTempPath
		{
			get { return WorkingAreaPath + @"\temp\"; }
		}
	}
}
