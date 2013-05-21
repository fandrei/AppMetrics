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
		public static void DeleteAllFiles(string path)
		{
			var repeatCount = 10;

			while (true)
			{
				try
				{
					foreach (var file in Directory.GetFiles(path))
					{
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
