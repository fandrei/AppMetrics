using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AppMetrics.Shared
{
	public static class Util
	{
		public static string Escape(string val)
		{
			var res = val.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
			return res;
		}

		public static string GetAppLocation()
		{
			var location = Assembly.GetExecutingAssembly().CodeBase;
			location = (new Uri(location)).LocalPath;
			var res = Path.GetDirectoryName(location) + "\\";
			return res;
		}

		public static string Serialize(DateTime val)
		{
			return val.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
		}

		public static DateTime ParseDateTime(string val)
		{
			var formats = new[]
				{
					"yyyy-MM-dd HH:mm:ss.fffffff",
					"yyyy-MM-dd HH:mm:ss",
					"yyyy-MM-dd",
					"u",
				};
			DateTime res;
			if (!DateTime.TryParseExact(val, formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out res))
			{
				var message = string.Format("Invalid datetime value: \"{0}\"", val);
				throw new ArgumentException(message);
			}
			return res;
		}

		public static string Format(DateTime val)
		{
			var res = val.ToString("yyyy-MM-dd HH:mm:ss");
			return res;
		}

		public static bool IsNullOrEmpty(this string val)
		{
			return string.IsNullOrEmpty(val);
		}
	}
}