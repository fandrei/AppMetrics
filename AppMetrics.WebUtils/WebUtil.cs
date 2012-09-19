using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;

namespace AppMetrics.WebUtils
{
	public static class WebUtil
	{
		public static void CheckIpAddress()
		{
			var request = HttpContext.Current.Request;
			if (!request.IsLocal)
			{
				var response = HttpContext.Current.Response;
				response.Write("Access from this IP address is not allowed");
				response.Status = "401 Unauthorized";
				response.StatusCode = 401;
				response.End();
				throw new UnauthorizedAccessException();
			}
		}

		public static string AppDataPath
		{
			get
			{
				var res = ResolvePath("~/App_Data");
				return res;
			}
		}

		public static string ResolvePath(string val)
		{
			if (val.Contains(':')) // an absolute path
				return val;

			if (val.StartsWith("~")) // site path
				return Path.GetFullPath(GetWebAppPath() + val.Substring(1));

			if (val.StartsWith(".")) // relative path
				return Path.GetFullPath(GetWebAppPath() + "\\" + val);

			throw new ArgumentException();
		}

		public static void TryEnableCompression(HttpContext context)
		{
			var acceptEncoding = context.Request.Headers["Accept-Encoding"];
			if (!string.IsNullOrEmpty(acceptEncoding))
			{
				acceptEncoding = acceptEncoding.ToLower();

				var response = context.Response;
				var prevUncompressedStream = response.Filter;

				if (acceptEncoding.Contains("deflate") || acceptEncoding == "*")
				{
					response.Filter = new DeflateStream(prevUncompressedStream, CompressionMode.Compress);
					response.AppendHeader("Content-Encoding", "deflate");
				}
				else if (acceptEncoding.Contains("gzip"))
				{
					response.Filter = new GZipStream(prevUncompressedStream, CompressionMode.Compress);
					response.AppendHeader("Content-Encoding", "gzip");
				}
			}
		}

		public static string GetWebAppPath()
		{
			var location = Assembly.GetExecutingAssembly().CodeBase;
			location = (new Uri(location)).LocalPath;
			var res = Path.GetDirectoryName(location) + "\\";
			res = Path.GetFullPath(res + "..");
			return res;
		}
	}
}