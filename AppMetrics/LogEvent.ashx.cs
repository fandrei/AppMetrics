using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace AppMetrics
{
	/// <summary>
	/// Summary description for LogEvent
	/// </summary>
	public class LogEvent : IHttpHandler
	{
		static void Init()
		{
			lock (Sync)
			{
				if (_logFile == null)
				{
					var logPath = Path.Combine(AppSettings.AppDataPath, Const.LogFileName);
					_logFile = new StreamWriter(logPath, true, Encoding.UTF8) { AutoFlush = true };
				}
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

				var sessionId = context.Request.Params["MessageSession"];
				if (string.IsNullOrEmpty(sessionId))
					throw new ApplicationException("No session ID");

				var filePath = GetDataFilePath(applicationKey, sessionId);
				var fileExisted = File.Exists(filePath);

				using (var writer = new StreamWriter(filePath, true, Encoding.UTF8))
				{
					var name = context.Request.Params["MessageName"];
					var data = context.Request.Params["MessageData"];
					var clientTime = context.Request.Params["MessageTime"];

					if (!fileExisted)
					{
						writer.WriteLine("{0}\t{1}\t{2}", clientTime, "ClientIP", context.Request.UserHostAddress);
						writer.WriteLine("{0}\t{1}\t{2}", clientTime, "ClientHostName", context.Request.UserHostName);
						writer.WriteLine("{0}\t{1}\t{2}", clientTime, "ClientUserAgent", context.Request.UserAgent);
					}

					data = Util.Escape(data);
					writer.WriteLine("{0}\t{1}\t{2}", clientTime, name, data);
				}
			}
			catch (Exception exc)
			{
				ReportLog(exc);
#if DEBUG
				context.Response.Write(exc);
#endif
			}
		}

		private static string GetDataFilePath(string applicationKey, string sessionId)
		{
			var basePath = AppSettings.DataStoragePath;
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
				var time = DateTime.UtcNow.ToString("u");
				time = time.Replace(':', '_');
				var filePath = Path.GetFullPath(string.Format("{0}\\{1}.{2}.txt", dataRootPath, time, sessionId));
				if (!filePath.StartsWith(dataRootPath)) // block malicious session ids
					throw new ArgumentException(filePath);
				return filePath;
			}
		}

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}

		enum Priority { Low, High }

		static void ReportLog(object val, Priority priority = Priority.High)
		{
			try
			{
				var text = val.ToString();

				if (priority != Priority.Low)
					EventLog.WriteEntry(Const.EventLogSourceName, text);

				if (_logFile != null)
				{
					var time = DateTime.UtcNow;
					bool multiLineData = text.Contains('\n');
					if (multiLineData)
					{
						_logFile.WriteLine(time);
						_logFile.WriteLine(Const.Delimiter);
						_logFile.WriteLine(text);
						_logFile.WriteLine(Const.Delimiter);
					}
					else
					{
						_logFile.WriteLine("{0}\t{1}", time, text);
					}
				}
			}
			catch (Exception exc)
			{
				Trace.WriteLine(val);
				Trace.WriteLine(exc);
			}
		}

		private static StreamWriter _logFile;
		static readonly object Sync = new object();
	}
}