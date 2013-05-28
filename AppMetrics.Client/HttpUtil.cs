using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace AppMetrics.Client
{
	public static class HttpUtil
	{
		public static string Request(string url, string method = "GET", Dictionary<string, string> args = null, ICredentials credentials = null)
		{
			var buf = new StringBuilder();
			if (args != null)
			{
				foreach (var val in args)
				{
					buf.AppendFormat("{0}={1}&", Uri.EscapeUriString(val.Key), Uri.EscapeUriString(val.Value));
				}
			}

			if (method == "GET" && buf.Length > 0)
			{
				if (url.Contains("?"))
					url += "&";
				else
					url += "?";
				url += buf;
			}

			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = method;
			request.Timeout = 5000;
			request.ReadWriteTimeout = request.Timeout;
			request.Credentials = credentials;

			if (method == "GET")
			{
				request.ContentType = "text/plain; encoding='utf-8'";
			}
			else if (method == "POST")
			{
				request.ContentType = "application/x-www-form-urlencoded; encoding='utf-8'";

				using (var stream = request.GetRequestStream())
				{
					using (var writer = new StreamWriter(stream, Encoding.UTF8))
					{
						writer.Write(buf.ToString());
					}
				}
			}

			var response = request.GetResponse();
			using (var responseStream = response.GetResponseStream())
			{
				using (var reader = new StreamReader(responseStream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}

		public static string Request(string url, ICredentials credentials)
		{
			return Request(url, "GET", null, credentials);
		}
	}
}
