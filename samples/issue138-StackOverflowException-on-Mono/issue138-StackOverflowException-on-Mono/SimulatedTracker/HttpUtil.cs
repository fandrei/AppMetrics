﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace issue138StackOverflowExceptiononMono
{
	public static class HttpUtil
	{
		private const int DefaultTimeout = 100 * 1000;

		public static string Request(string url, Dictionary<string, string> args = null, string method = "GET", 
			int timeout = DefaultTimeout, ICredentials credentials = null)
		{
			var buf = new StringBuilder();
			if (args != null)
			{
				foreach (var val in args)
				{
					buf.AppendFormat("{0}={1}&", Uri.EscapeDataString(val.Key), Uri.EscapeDataString(val.Value));
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

//			var request = (HttpWebRequest)WebRequest.Create(url);
//			request.Method = method;
//			request.Timeout = timeout;
//			request.ReadWriteTimeout = request.Timeout;
//			request.Credentials = credentials;
//
//			if (method == "GET")
//			{
//				request.ContentType = "text/plain; encoding='utf-8'";
//			}
//			else if (method == "POST")
//			{
//				request.ContentType = "application/x-www-form-urlencoded; encoding='utf-8'";
//
//				using (var stream = request.GetRequestStream())
//				{
//					using (var writer = new StreamWriter(stream)) // UTF8 without BOM
//					{
//						writer.Write(buf.ToString());
//					}
//				}
//			}

			Console.WriteLine ("{0} {1}", method, url);

			if (args != null)
			{
				foreach (var val in args)
				{
					Console.WriteLine("\t{0}={1}", val.Key, val.Value);
				}
			}

//			using (var response = request.GetResponse())
//			using (var responseStream = response.GetResponseStream())
//			using (var reader = new StreamReader(responseStream, Encoding.UTF8))
//			{
//				return reader.ReadToEnd();
//			}
			return "OK";
		}

		public static string Request(string url, ICredentials credentials, Dictionary<string, string> args = null, int timeout = DefaultTimeout)
		{
			return Request(url, args, "GET", timeout, credentials);
		}
	}
}
