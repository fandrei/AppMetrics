using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AppMetrics.AgentService
{
	public static class FileUtil
	{
		public static void SetAttributes(string fileName, FileAttributes attr)
		{
			File.SetAttributes(fileName, File.GetAttributes(fileName) | attr);
		}

		public static void ResetAttributes(string fileName, FileAttributes attr)
		{
			if (HasAttributes(fileName, attr))
				File.SetAttributes(fileName, File.GetAttributes(fileName) & ~attr);
		}

		public static bool HasAttributes(string fileName, FileAttributes attr)
		{
			return ((File.GetAttributes(fileName) & attr) == attr);
		}

		public static void DeleteAllFiles(string path)
		{
			var repeatCount = 10;

			while (true)
			{
				try
				{
					foreach (var file in Directory.GetFiles(path))
					{
						ResetAttributes(file, FileAttributes.ReadOnly);
						File.Delete(file);
					}
					break;
				}
				catch (UnauthorizedAccessException exc)
				{
					repeatCount--;
					if (repeatCount <= 0)
						throw;
				}
				Thread.Sleep(TimeSpan.FromSeconds(1));
			}
		}
	}
}
