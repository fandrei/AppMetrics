﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.VisualBasic.Devices;

namespace AppMetrics.Client
{
	public class Tracker : IDisposable
	{
		public Tracker(string url, string applicationKey)
		{
			if (string.IsNullOrEmpty(url))
				throw new ArgumentNullException();
			_url = url;

			if (string.IsNullOrEmpty(applicationKey))
				throw new ArgumentNullException();
			_applicationKey = applicationKey;

			SessionId = Guid.NewGuid().ToString();

			lock (Sync)
			{
				Sessions.Add(this);
			}
		}

		public void Dispose()
		{
			Log("SessionFinished", null, MessageSeverity.High);
			lock (Sync)
			{
				Sessions.Remove(this);
			}
		}

		static Tracker()
		{
			LoggingThread.Start();
		}

		public static void Terminate(bool waitAll = false)
		{
			lock (Sync)
			{
				var sessionsTmp = new HashSet<Tracker>(Sessions);
				foreach (var session in sessionsTmp)
				{
					session.Dispose();
				}
			}
			_terminated = true;
			var period = waitAll ? Timeout.Infinite : 5 * 1000;
			LoggingThread.Join(period);
			LoggingThread.Abort();
		}

		public void Log(string name, object val, MessageSeverity severity = MessageSeverity.Low)
		{
			lock (Sync)
			{
				try
				{
					ReportSystemInfo();
				}
				catch (Exception exc)
				{
					Log("Exception", exc);
				}

				if (Messages.Count >= MaxMessagesCount)
				{
					var tmp = new List<MessageInfo>(Messages);
					Messages.Clear();
					tmp.RemoveAll(message => message.Severity == MessageSeverity.Low);
					foreach (var cur in tmp)
						Messages.Enqueue(cur);

					AddMessage(WarningName, "Message queue overflow. Some messages are skipped.", MessageSeverity.High);
					if (Messages.Count >= MaxMessagesCount) // too much high-priority messages
					{
						Messages.Clear();
						AddMessage(ErrorName, "Critical message queue overflow. All messages are removed.", MessageSeverity.High);
					}
				}

				AddMessage(name, val, severity);
			}
		}

		private const string WarningName = "AppMetrics.Warning";
		private const string ErrorName = "AppMetrics.Error";

		private void AddMessage(string name, object val, MessageSeverity severity)
		{
			if (_terminated)
				return;

			if (val == null)
				val = "";

			lock (Sync)
			{
				Messages.Enqueue(
					new MessageInfo
						{
							ApplicationKey = _applicationKey,
							Name = name,
							Value = val.ToString(),
							SessionId = SessionId,
							Url = _url,
							Time = DateTime.Now,
							Severity = severity
						});
			}
		}

		static void LoggingThreadEntry()
		{
			try
			{
				while (!_terminated)
				{
					SendMessages();
					Thread.Sleep(TimeSpan.FromMilliseconds(100));
				}

				SendMessages();
			}
			catch (ThreadAbortException)
			{ }
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
			}
		}

		private static void SendMessages()
		{
			try
			{
				ReportPeriodicInfoAllSessions();

				using (var client = new WebClient())
				{
					while (true)
					{
						MessageInfo message;
						lock (Sync)
						{
							if (Messages.Count == 0)
								return;
							message = Messages.Peek();
						}

						SendMessage(client, message);

						lock (Sync)
						{
							Messages.Dequeue();
						}
					}
				}
			}
			catch (ThreadInterruptedException)
			{ }
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
			}
		}

		static void SendMessage(WebClient client, MessageInfo message)
		{
			var vals = new NameValueCollection
				{
					{ "MessageAppKey", message.ApplicationKey }, 
					{ "MessageSession", message.SessionId }, 
					{ "MessageName", message.Name },
					{ "MessageData", message.Value },
					{ "MessageTime", message.Time.ToString("yyyy-MM-dd HH:mm:ss.fffffff") },
				};

			var response = client.UploadValues(message.Url, "POST", vals);
			CountNewRequest();
			var responseText = Encoding.ASCII.GetString(response);
			if (!string.IsNullOrEmpty(responseText))
				throw new ApplicationException(responseText);
		}

		static void CountNewRequest()
		{
			Interlocked.Increment(ref _requestsSent);
		}

		public static long GetServedRequestsCount()
		{
			return _requestsSent;
		}

		private bool _systemInfoIsReported;

		void ReportSystemInfo()
		{
			lock (Sync)
			{
				if (_systemInfoIsReported)
					return;
				_systemInfoIsReported = true;
			}

			var computerInfo = new ComputerInfo();

			Log("System_OsName", computerInfo.OSFullName);
			Log("System_OsVersion", Environment.OSVersion.VersionString);

			Log("System_ComputerName", Environment.MachineName);
			Log("System_UserName", Environment.UserName);

			Log("System_ClrVersion", Environment.Version.ToString());

			Log("System_PhysicalMemory", ToMegabytes(computerInfo.TotalPhysicalMemory));
			Log("System_VirtualMemory", ToMegabytes(computerInfo.TotalVirtualMemory));

			Log("System_CurrentCulture", Thread.CurrentThread.CurrentCulture.Name);
			Log("System_CurrentUiCulture", Thread.CurrentThread.CurrentUICulture.Name);

			Log("System_SystemDefaultEncoding", Encoding.Default.WebName);

			Log("System_CalendarType", computerInfo.InstalledUICulture.Calendar.GetType().Name);

			Log("System_NumberDecimalSeparator", computerInfo.InstalledUICulture.NumberFormat.NumberDecimalSeparator);

			var timeZone = TimeZone.CurrentTimeZone;
			Log("System_TimeZone", timeZone.StandardName);

			var offset = timeZone.GetUtcOffset(DateTime.Now);
			Log("System_TimeZoneOffset", offset.TotalHours);

			var processFile = Process.GetCurrentProcess().MainModule.FileName;
			Log("Client_ProcessName", processFile);

			var processVersion = FileVersionInfo.GetVersionInfo(processFile).FileVersion;
			Log("Client_ProcessVersion", processVersion);
		}

		static void ReportPeriodicInfoAllSessions()
		{
			lock (Sync)
			{
				if (DateTime.UtcNow - _lastSentPeriodic < PeriodicTime)
					return;

				foreach (var session in Sessions)
				{
					session.ReportPeriodicInfo();
				}
				_lastSentPeriodic = DateTime.UtcNow;
			}
		}

		private void ReportPeriodicInfo()
		{
			var workingSet = ToMegabytes((ulong)Process.GetCurrentProcess().WorkingSet64);

			Log("Client_CurrentWorkingSet", workingSet);

			var computerInfo = new ComputerInfo();

			Log("System_AvailablePhysicalMemory", ToMegabytes(computerInfo.AvailablePhysicalMemory));
			Log("System_AvailableVirtualMemory", ToMegabytes(computerInfo.AvailableVirtualMemory));
		}

		static ulong ToMegabytes(ulong val)
		{
			return val / (1024 * 1024);
		}

		public string SessionId { get; private set; }
		private readonly string _url;
		private readonly string _applicationKey;

		private static readonly object Sync = new object();
		private static readonly Queue<MessageInfo> Messages = new Queue<MessageInfo>(MaxMessagesCount);
		private const int MaxMessagesCount = 4096;
		private static readonly HashSet<Tracker> Sessions = new HashSet<Tracker>();
		private static readonly Thread LoggingThread = new Thread(LoggingThreadEntry);
		private static DateTime _lastSentPeriodic;
		private static readonly TimeSpan PeriodicTime = TimeSpan.FromMinutes(10);

		private static long _requestsSent;
		private static volatile bool _terminated;
	}
}
