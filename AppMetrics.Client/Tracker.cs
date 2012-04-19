using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
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
			if (!string.IsNullOrEmpty(_url) && url != _url)
				throw new InvalidOperationException();
			_url = url;

			if (string.IsNullOrEmpty(applicationKey))
				throw new ArgumentNullException();
			if (!string.IsNullOrEmpty(_applicationKey) && _applicationKey != applicationKey)
				throw new InvalidOperationException();
			_applicationKey = applicationKey;

			SessionId = Guid.NewGuid().ToString();

			lock (Sync)
			{
				Sessions.Add(this);
			}
			ReportPeriodicInfo();
		}

		public void Dispose()
		{
			Log("SessionFinished", null, MessagePriority.High);
			lock (Sync)
			{
				Sessions.Remove(this);
			}
		}

		static Tracker()
		{
			lock (Sync)
			{
				_lastSentPeriodic = DateTime.UtcNow;
			}
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

		public void Log(string name, object val, MessagePriority priority = MessagePriority.Low)
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
					Messages.RemoveAll(message => message.Priority == MessagePriority.Low);

					AddMessage(WarningName, "Message queue overflow. Some messages are skipped.", MessagePriority.High);
					if (Messages.Count >= MaxMessagesCount) // too much high-priority messages
					{
						Messages.Clear();
						AddMessage(ErrorName, "Critical message queue overflow. All messages are removed.", MessagePriority.High);
					}
				}

				AddMessage(name, val, priority);
			}
		}

		private const string WarningName = "AppMetrics.Warning";
		private const string ErrorName = "AppMetrics.Error";

		private void AddMessage(string name, object val, MessagePriority priority)
		{
			if (_terminated)
				return;

			if (val == null)
				val = "";

			if (val is double)
				val = ((double)val).ToString(CultureInfo.InvariantCulture);
			else if (val is float)
				val = ((float)val).ToString(CultureInfo.InvariantCulture);
			else if (val is decimal)
				val = ((decimal)val).ToString(CultureInfo.InvariantCulture);

			lock (Sync)
			{
				Messages.Add(
					new MessageInfo
						{
							Name = name,
							Value = Util.Escape(val.ToString()),
							SessionId = SessionId,
							Time = DateTime.UtcNow,
							Priority = priority
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
					Thread.Sleep(SendingPeriod);
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
						if (_packet.Length == 0)
						{
							lock (Sync)
							{
								if (Messages.Count == 0)
									return;

								int i = 0;
								for (; i < Messages.Count; i++)
								{
									var message = Messages[i];
									var cur = string.Format("{0}\t{1}\t{2}\t{3}\r\n", message.SessionId,
										message.Time.ToString("yyyy-MM-dd HH:mm:ss.fffffff"), message.Name, message.Value);

									if (_packet.Length + cur.Length > _packet.Capacity)
										break;
									_packet.Append(cur);
								}

								var messagesSent = i;
								Messages.RemoveRange(0, messagesSent);
							}
						}

						SendPacket(client, _packet.ToString());
						_packet.Clear(); // clear packet if succeeded
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

		static void SendPacket(WebClient client, string packet)
		{
			var vals = new NameValueCollection
				{
					{ "MessageAppKey", _applicationKey },
					{ "MessagesList", packet }, 
				};

			var response = client.UploadValues(_url, "POST", vals);
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

			Log("System_CalendarType", CultureInfo.CurrentCulture.Calendar.GetType().Name);

			Log("System_NumberDecimalSeparator", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

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
			Log("Client_WorkingSet", workingSet);

			var privateMemorySize = ToMegabytes((ulong)Process.GetCurrentProcess().PrivateMemorySize64);
			Log("Client_PrivateMemorySize", privateMemorySize);

			var computerInfo = new ComputerInfo();

			Log("System_AvailablePhysicalMemory", ToMegabytes(computerInfo.AvailablePhysicalMemory));
			Log("System_AvailableVirtualMemory", ToMegabytes(computerInfo.AvailableVirtualMemory));
		}

		static ulong ToMegabytes(ulong val)
		{
			return val / (1024 * 1024);
		}

		private static string _url;

		public string SessionId { get; private set; }
		private static string _applicationKey;

		private static readonly object Sync = new object();
		private static readonly List<MessageInfo> Messages = new List<MessageInfo>(MaxMessagesCount);
		private static StringBuilder _packet = new StringBuilder(MaxPacketSize);
		private static readonly HashSet<Tracker> Sessions = new HashSet<Tracker>();
		private static readonly Thread LoggingThread = new Thread(LoggingThreadEntry);

		private const int MaxMessagesCount = 4096;
		private const int MaxPacketSize = 1024 * 16;
		private static readonly TimeSpan SendingPeriod = TimeSpan.FromSeconds(0.5);

		private static DateTime _lastSentPeriodic;
		private static readonly TimeSpan PeriodicTime = TimeSpan.FromMinutes(10);

		private static long _requestsSent;
		private static volatile bool _terminated;
	}
}
