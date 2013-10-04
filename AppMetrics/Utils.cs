using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

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
			var doesNotExist = false;
			var unauthorized = false;

			try
			{
				_mutex = Mutex.OpenExisting(id, MutexRights.Synchronize | MutexRights.Modify);
			}
			catch (WaitHandleCannotBeOpenedException)
			{
				doesNotExist = true;
			}
			catch (UnauthorizedAccessException ex)
			{
				unauthorized = true;
			}

			if (doesNotExist)
			{
				_mutex = new Mutex(false, id);

				var allowEveryoneRule = new MutexAccessRule(
					new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
				var securitySettings = new MutexSecurity();
				securitySettings.AddAccessRule(allowEveryoneRule);
				_mutex.SetAccessControl(securitySettings);
			}
			else if (unauthorized)
			{
				var tempMutex = Mutex.OpenExisting(id, MutexRights.ReadPermissions | MutexRights.ChangePermissions);
				var securitySettings = tempMutex.GetAccessControl();

				var user = Environment.UserDomainName + "\\" + Environment.UserName;

				// the rule that denied the current user the right to enter and release the mutex must be removed
				var rule = new MutexAccessRule(user, MutexRights.Synchronize | MutexRights.Modify, AccessControlType.Deny);
				securitySettings.RemoveAccessRule(rule);

				// Now grant the correct rights
				var allowEveryoneRule = new MutexAccessRule(
					new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
				securitySettings.AddAccessRule(allowEveryoneRule);
				tempMutex.SetAccessControl(securitySettings);

				_mutex = Mutex.OpenExisting(id, MutexRights.Synchronize | MutexRights.Modify);
			}

			var success = _mutex.WaitOne(TimeSpan.FromSeconds(10), false);
			if (success == false)
			{
				_mutex.Dispose();
				_mutex = null;
				throw new ApplicationException(string.Format("Can't lock mutex (timed out): {0}", id));
			}
		}

		public void Dispose()
		{
			if (_mutex != null)
			{
				try
				{
					_mutex.ReleaseMutex();
				}
				catch (Exception exc)
				{
					Trace.WriteLine(exc);
				}

				_mutex.Dispose();
			}
		}

		private readonly Mutex _mutex;
	}

}