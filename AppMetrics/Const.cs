using System;
using System.Collections.Generic;
using System.Linq;

namespace AppMetrics
{
	public static class Const
	{
		public const string AppName = "AppMetrics";

		public const string LogFileName = "AppMetrics.Log.txt";
		public const string EventLogSourceName = "AppMetricsEventSource";
		public static readonly string Delimiter = new string('-', 80);

		public static readonly byte[] Utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
	}

	public enum LogPriority { Low, High }
}