using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Timer = System.Timers.Timer;

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
				if (_timer == null)
				{
					_timer = new Timer { Interval = 1000, AutoReset = true};
					_timer.Elapsed += OnTimer;
					_timer.Start();

					if (_logFile == null)
					{
						var logPath = Path.Combine(GetDataFolderPath(HttpContext.Current), EventLogFileName);
						_logFile = new StreamWriter(logPath, true, Encoding.UTF8);
						_logFile.AutoFlush = true;
					}
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

				var filePath = GetDataFilePath(context, applicationKey, sessionId);

				using (var writer = new StreamWriter(filePath, true, Encoding.UTF8))
				{
					var name = context.Request.Params["MessageName"];
					var data = context.Request.Params["MessageData"];
					var clientTime = context.Request.Params["MessageTime"];

					bool multiLineData = data.Contains('\n');
					if (multiLineData)
					{
						writer.WriteLine("{0}\t{1}", clientTime, name);
						writer.WriteLine(Delimiter);
						writer.WriteLine(data);
						writer.WriteLine(Delimiter);
					}
					else
					{
						writer.WriteLine("{0}\t{1}\t{2}", clientTime, name, data);
					}
				}

				CountNewRequest();
			}
			catch (Exception exc)
			{
				Report(exc);
#if DEBUG
				context.Response.Write(exc);
#endif
			}
		}

		private static string GetDataFilePath(HttpContext context, string applicationKey, string sessionId)
		{
			var basePath = GetDataFolderPath(context);
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

		private static string GetDataFolderPath(HttpContext context)
		{
			return Path.GetFullPath(context.Request.PhysicalApplicationPath + "\\Data");
		}

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}

		static void CountNewRequest()
		{
			Interlocked.Increment(ref _requestCounter);
		}

		static void OnTimer(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				var count = Interlocked.Exchange(ref _requestCounter, 0);
				if (count != 0)
					Report(string.Format("Requests per second: {0}", count), Priority.Low);
			}
			catch (Exception exc)
			{
				Report(exc);
			}
		}

		enum Priority { Low, High }

		static void Report(object val, Priority priority = Priority.High)
		{
			try
			{
				var text = val.ToString();

				if (priority != Priority.Low)
					EventLog.WriteEntry(EventLogSourceName, text);

				if (_logFile != null)
				{
					var time = DateTime.Now;
					bool multiLineData = text.Contains('\n');
					if (multiLineData)
					{
						_logFile.WriteLine(time);
						_logFile.WriteLine(Delimiter);
						_logFile.WriteLine(text);
						_logFile.WriteLine(Delimiter);
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

		private const string EventLogSourceName = "AppMetricsEventSource";

		private static StreamWriter _logFile;
		private const string EventLogFileName = "AppMetrics.Log.txt";

		private static readonly string Delimiter = new string('-', 80);
		private static long _requestCounter;
		private static Timer _timer;
		static readonly object Sync = new object();
	}
}