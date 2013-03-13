using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace AppMetrics
{
	static class Utils
	{
		public static Mutex TryLockFile(string sessionId, string filePath)
		{
			var mutex = new Mutex(false, Const.GetFileMutexName(sessionId));
			if (!mutex.WaitOne(TimeSpan.FromSeconds(10)))
			{
				mutex.Dispose();
				throw new ApplicationException(string.Format("Can't open file (access locked): {0}", filePath));
			}
			return mutex;
		}
	}
}