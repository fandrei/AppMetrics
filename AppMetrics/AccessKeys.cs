using System;
using System.Collections.Generic;
using System.Linq;

namespace AppMetrics
{
	public static class AccessKeys
	{
		public static void VerifyAccess(string key)
		{
			if (string.IsNullOrWhiteSpace(key) || !IsAllowed(key))
				throw new ApplicationException("Wrong access key");
		}

		public static bool IsAllowed(string key)
		{
			lock (Sync)
			{
				if (!AppSettings.Instance.RequireAccessKey)
					return true;

				if (_keys == null)
				{
					_keys = AppSettings.Instance.AccessKeys;
				}
				var keyIsPresent = _keys.Contains(key);
				return keyIsPresent;
			}
		}

		private static string[] _keys;
		static readonly object Sync = new object();
	}
}