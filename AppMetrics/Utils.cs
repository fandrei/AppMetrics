using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Web;

namespace AppMetrics
{
	static class Utils
	{
		public static MutexLocker TryLockFile(string sessionId)
		{
			return new MutexLocker(Const.GetFileMutexName(sessionId));
		}
	}

	public class MutexLocker : IDisposable
	{
		public MutexLocker(string id)
		{
			_mutex = new Mutex(false, id);

			var allowEveryoneRule = new MutexAccessRule(
				new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
			var securitySettings = new MutexSecurity();
			securitySettings.AddAccessRule(allowEveryoneRule);
			_mutex.SetAccessControl(securitySettings);

			try
			{
				_hasHandle = _mutex.WaitOne(TimeSpan.FromSeconds(10), false);
				if (_hasHandle == false)
					throw new ApplicationException(string.Format("Can't lock mutex: {0}", id));
			}
			catch (AbandonedMutexException)
			{
				_hasHandle = true;
			}
		}

		public void Dispose()
		{
			if (_mutex != null)
			{
				if (_hasHandle)
					_mutex.ReleaseMutex();

				_mutex.Dispose();
			}
		}

		private readonly Mutex _mutex;
		private bool _hasHandle;
	}

}