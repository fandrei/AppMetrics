using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

using AppMetrics.WebUtils;

namespace AppMetrics
{
	/// <summary>
	/// Summary description for LogEvent
	/// </summary>
	public class LogEvent : IHttpHandler
	{
		public static void Init()
		{
			lock (Sync)
			{
				if (AppSettings.Instance == null)
					AppSettings.Load(SiteConfig.DataStoragePath);
			}
		}

		public void ProcessRequest(HttpContext context)
		{
			try
			{
				Init();

				var applicationKey = context.Request.Params["MessageAppKey"];
				if (string.IsNullOrEmpty(applicationKey))
					throw new ApplicationException("No application key");

				var accessKey = context.Request.Params["AccessKey"];
				if (string.IsNullOrEmpty(accessKey))
				{
					var appKeyParts = applicationKey.Split('|');
					if (appKeyParts.Length == 2)
					{
						applicationKey = appKeyParts[0];
						accessKey = appKeyParts[1];
					}
				}
				AccessKeys.VerifyAccess(accessKey);

				// NOTE that client side has to escape data if it contains the same char that is used as line separator char
				var tmp = context.Request.Params["LineSeparator"];
				if (string.IsNullOrEmpty(tmp))
					tmp = "\t";
				if (tmp.Length > 1)
					throw new ApplicationException("Invalid line separator");
				var separator = tmp[0];

				var messages = context.Request.Params["MessagesList"];
				if (!string.IsNullOrEmpty(messages))
				{
					var sessionId = context.Request.Params["MessageSession"];
					if (string.IsNullOrEmpty(sessionId))
						ProcessMessages(context.Request, applicationKey, messages, separator);
					else
						ProcessMessages(context.Request, applicationKey, sessionId, messages, separator);
				}
				else
					ProcessMessage(context, applicationKey);
			}
			catch (Exception exc)
			{
				ReportLog(exc);
#if DEBUG
				context.Response.Write(exc);
#endif
			}
		}

		#region Multi-message mode

		private static void ProcessMessages(HttpRequest request, string applicationKey, string messagesText, char separator)
		{
			var textLines = messagesText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			var itemsByLines = textLines.Select(line => line.Split(new[] { separator })).ToArray();
			var tmp = itemsByLines.GroupBy(line => line[0], line => line.Skip(1).ToArray()).
				ToDictionary(group => group.Key, group => group.ToArray());
			var messagesBySessions = new Dictionary<string, string[][]>(tmp);

			foreach (var pair in messagesBySessions)
			{
				var sessionId = pair.Key;
				var lines = pair.Value;
				if (lines.Count() == 0)
					continue;

				if (lines.Any(line => line.Length != 3))
					throw new ApplicationException("Invalid count of items in the line");
				WriteData(request, applicationKey, sessionId, lines);
			}
		}

		private static void ProcessMessages(HttpRequest request,
			string applicationKey, string sessionId, string messagesText, char separator)
		{
			var textLines = messagesText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			var lines = textLines.Select(line => line.Split(new[] { separator })).ToArray();
			if (lines.Any(line => line.Length != 3))
				throw new ApplicationException("Invalid count of items in the line");

			WriteData(request, applicationKey, sessionId, lines);
		}

		private static void WriteData(HttpRequest request, string appKey, string sessionId, string[][] lines)
		{
			var filePath = GetDataFilePath(appKey, sessionId);

			using (var mutex = new Mutex(false, "AppMetrics:" + sessionId))
			{
				if (!mutex.WaitOne(TimeSpan.FromSeconds(5)))
					throw new ApplicationException(string.Format("Can't open file: {0}", filePath));

				WriteDataRaw(request, filePath, lines);
			}
		}

		private static void WriteDataRaw(HttpRequest request, string filePath, string[][] lines)
		{
			if (lines.Length == 0)
				return;

			using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
			{
				using (var writer = new StreamWriter(stream)) // by default, encoding is UTF8 without BOM
				{
					var fileExisted = writer.BaseStream.Length > 0;
					if (fileExisted)
					{
						writer.BaseStream.Seek(0, SeekOrigin.End);
					}
					else
					{
						writer.BaseStream.Write(Const.Utf8Bom, 0, Const.Utf8Bom.Length);
						var clientTime = lines[0][0];

						writer.WriteLine("{0}\t{1}\t{2}", clientTime, "ClientIP", request.UserHostAddress);
						writer.WriteLine("{0}\t{1}\t{2}", clientTime, "ClientHostName", request.UserHostName);
						writer.WriteLine("{0}\t{1}\t{2}", clientTime, "ClientUserAgent", request.UserAgent);
					}

					var buf = new StringBuilder();
					foreach (var item in lines)
					{
						var line = string.Join("\t", item);
						buf.AppendLine(line);
					}

					writer.Write(buf);
				}
			}
		}

		#endregion

		#region Single-message mode

		private static void ProcessMessage(HttpContext context, string applicationKey)
		{
			var sessionId = context.Request.Params["MessageSession"];
			if (string.IsNullOrEmpty(sessionId))
				throw new ApplicationException("No session ID");

			var filePath = GetDataFilePath(applicationKey, sessionId);
			WriteData(filePath, context);
		}

		private static void WriteData(string filePath, HttpContext context)
		{
			var name = context.Request.Params["MessageName"];
			var data = context.Request.Params["MessageData"];
			if (string.IsNullOrEmpty(data))
				return;

			var clientTime = context.Request.Params["MessageTime"];

			using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
			{
				using (var writer = new StreamWriter(stream)) // by default, encoding is Encoding.UTF8 without BOM
				{
					var fileExisted = writer.BaseStream.Length > 0;
					writer.BaseStream.Seek(0, SeekOrigin.End);

					if (!fileExisted)
					{
						writer.BaseStream.Write(Const.Utf8Bom, 0, Const.Utf8Bom.Length);

						writer.WriteLine("{0}\t{1}\t{2}", clientTime, "ClientIP", context.Request.UserHostAddress);
						writer.WriteLine("{0}\t{1}\t{2}", clientTime, "ClientHostName", context.Request.UserHostName);
						writer.WriteLine("{0}\t{1}\t{2}", clientTime, "ClientUserAgent", context.Request.UserAgent);
					}

					data = Shared.Util.Escape(data);
					writer.WriteLine("{0}\t{1}\t{2}", clientTime, name, data);
				}
			}
		}

		#endregion

		private static string GetDataFilePath(string applicationKey, string sessionId)
		{
			var basePath = SiteConfig.DataStoragePath;
			var dataRootPath = Path.Combine(basePath, applicationKey);
			if (!dataRootPath.StartsWith(basePath)) // block malicious application keys
				throw new ArgumentException(dataRootPath);

			if (!Directory.Exists(dataRootPath))
				Directory.CreateDirectory(dataRootPath);

			var filesMask = string.Format("*.{0}.txt", sessionId);
			var files = Directory.GetFiles(dataRootPath, filesMask);
			if (files.Length > 0)
			{
				if (files.Length != 1)
					throw new ApplicationException(string.Format("Too much data files for session {0}", sessionId));
				return files[0];
			}
			else
			{
				var time = DateTime.UtcNow;
				var filePath = Const.FormatFilePath(dataRootPath, sessionId, time);
				return filePath;
			}
		}

		public static void ReportLog(object val, LogPriority priority = LogPriority.Low)
		{
			try
			{
				var text = val.ToString();

				if (priority != LogPriority.Low)
					EventLog.WriteEntry(Const.EventLogSourceName, text);

				WebLogger.Report(val);
			}
			catch (Exception exc)
			{
				Trace.WriteLine(val);
				Trace.WriteLine(exc);
			}
		}

		static readonly object Sync = new object();

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}
	}
}