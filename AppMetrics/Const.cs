using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AppMetrics
{
	public static class Const
	{
		public const string AppName = "AppMetrics";

		public const string EventLogSourceName = "AppMetricsEventSource";
		public static readonly string Delimiter = new string('-', 80);

		public static readonly byte[] Utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };

		public static string FormatFilePath(string dataRootPath, string sessionId, DateTime time)
		{
			var timeText = time.ToString("yyyy-MM-dd HH_mm_ss");
			var filePath = Path.GetFullPath(string.Format("{0}\\{1}.{2}.txt", dataRootPath, timeText, sessionId));
			if (!filePath.StartsWith(dataRootPath)) // block malicious session ids
				throw new ArgumentException(filePath);
			return filePath;
		}

		public static string GetFileMutexName(string sessionId)
		{
			return "AppMetrics.File:" + sessionId;
		}
	}

	public enum LogPriority { Low, High }
}