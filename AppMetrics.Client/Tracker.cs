using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.VisualBasic.Devices;

namespace AppMetrics.Client
{
	public class Tracker : TrackerBase
	{
		public static Tracker Create(string url, string applicationKey, string accessKey)
		{
			lock (Sync)
			{
				if (_terminated)
					throw new InvalidOperationException();

				var found = Sessions.Where(
					session => !session._disposed && session.Url == url && session.ApplicationKey == applicationKey);
				if (found.FirstOrDefault() != null)
					return found.First();
				var res = new Tracker(url, applicationKey, accessKey);
				return res;
			}
		}

		private Tracker(string url, string applicationKey, string accessKey)
		{
			if (string.IsNullOrEmpty(url))
				throw new ArgumentNullException();
			Url = url;

			if (string.IsNullOrEmpty(applicationKey))
				throw new ArgumentNullException();
			ApplicationKey = applicationKey;

			AccessKey = accessKey;

			SessionId = Guid.NewGuid().ToString();

			lock (Sync)
			{
				Sessions.Add(this);
			}
			ReportPeriodicInfo();
		}

		public override void Dispose()
		{
			Log("SessionFinished", null, MessagePriority.High);
			_disposed = true;
		}

		private volatile bool _disposed;

		static Tracker()
		{
			lock (Sync)
			{
				_lastSentPeriodic = DateTime.UtcNow;
				LoggingThread.Name = "AppMetrics_MessageSending";
				LoggingThread.Start();
			}
		}

		public static void Terminate(bool waitAll = false)
		{
			try
			{
				lock (Sync)
				{
					foreach (var session in Sessions)
					{
						session.Dispose();
					}
				}

				_terminated = true;
				var period = waitAll ? Timeout.Infinite : 5 * 1000;
				LoggingThread.Join(period);
				LoggingThread.Abort();

				lock (Sync)
				{
					Sessions.Clear();
				}
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
			}
		}

		public override void FlushMessages()
		{
			SendMessages();
		}

		public override void Log(string name, string val, MessagePriority priority = MessagePriority.Low)
		{
			lock (Sync)
			{
				try
				{
					ReportSystemInfo();
				}
				catch (Exception exc)
				{
					Log(exc);
				}

				if (_messages.Count >= MaxMessagesCount)
				{
					_messages.RemoveAll(message => message.Priority == MessagePriority.Low);

					AddMessage(WarningName, "Message queue overflow. Some messages are skipped.", MessagePriority.High);
					if (_messages.Count >= MaxMessagesCount) // too much high-priority messages
					{
						_messages.Clear();
						AddMessage(ErrorName, "Critical message queue overflow. All messages are removed.", MessagePriority.High);
					}
				}

				AddMessage(name, val, priority);
			}
		}

		private const string WarningName = "AppMetrics.Warning";
		private const string ErrorName = "AppMetrics.Error";

		private void AddMessage(string name, string val, MessagePriority priority)
		{
			if (_disposed || _terminated)
			{
				Trace.WriteLine("WARNING.AppMetrics.Client: message ignored");
				return;
			}

			var valText = Escape(val ?? "");

			lock (Sync)
			{
				_messages.Add(
					new MessageInfo
						{
							Name = name,
							Value = valText,
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
					SendAllMessages();
					Thread.Sleep(SendingPeriod);
				}

				SendAllMessages();
			}
			catch (ThreadInterruptedException)
			{ }
			catch (ThreadAbortException)
			{ }
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
			}
		}

		private static void SendAllMessages()
		{
			Tracker[] sessions;
			lock (Sync)
			{
				sessions = Sessions.ToArray();
			}

			foreach (var session in sessions)
			{
				session.SendMessages();

				if (session._disposed)
				{
					lock (Sync)
					{
						Sessions.Remove(session);
					}
				}
			}
		}

		private void SendMessages()
		{
			lock (SendingSync)
			{
				try
				{
					ReportPeriodicInfoAllSessions();

					while (true)
					{
						string packet = null;
						lock (Sync)
						{
							if (_packet.Length == 0)
							{
								if (_messages.Count == 0)
									return;
								BuildPacket();
							}

							packet = _packet.ToString();
						}

						SendPacket(Url, AccessKey, ApplicationKey, packet);

						lock (Sync)
						{
							_packet.Clear(); // clear packet only if succeeded
						}
					}
				}
				catch (ThreadInterruptedException)
				{
				}
				catch (Exception exc)
				{
					Log("Exception", exc.ToString());
				}
			}
		}

		private void BuildPacket()
		{
			int i = 0;
			for (; i < _messages.Count; i++)
			{
				var message = _messages[i];
				var cur = string.Format("{0}\t{1}\t{2}\t{3}\r\n", message.SessionId,
					message.Time.ToString("yyyy-MM-dd HH:mm:ss.fffffff"), message.Name, message.Value);

				if (_packet.Length + cur.Length > _packet.Capacity)
					break;
				_packet.Append(cur);
			}

			var messagesSent = i;
			_messages.RemoveRange(0, messagesSent);
		}

		static void SendPacket(string url, string accessKey, string appKey, string packet)
		{
			var args = new Dictionary<string, string>()
				{
					{ "AccessKey", accessKey },
					{ "MessageAppKey", appKey },
					{ "MessagesList", packet }, 
				};

			var responseText = HttpUtil.Request(url, args, "POST");

			CountNewRequest();
			if (!string.IsNullOrEmpty(responseText))
				throw new ApplicationException(responseText);
		}

		static void CountNewRequest()
		{
			Interlocked.Increment(ref _requestsSent);
		}

		public static long GetServedRequestsCount()
		{
			var res = Interlocked.Read(ref _requestsSent);
			return res;
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

			try
			{
				var computerInfo = new ComputerInfo();

				Log("System_OsName", computerInfo.OSFullName);
				Log("System_OsVersion", Environment.OSVersion.VersionString);

				Log("System_ComputerName", Environment.MachineName);
				Log("System_UserName", Environment.UserName);

				Log("System_ClrVersion", Environment.Version.ToString());


				if (!IsUnderMono)
				{
					Log("System_PhysicalMemory", ToMegabytes(computerInfo.TotalPhysicalMemory));
					Log("System_VirtualMemory", ToMegabytes(computerInfo.TotalVirtualMemory));
				}
				else
				{
					using (var pc = new PerformanceCounter("Mono Memory", "Total Physical Memory"))
					{
						Log("System_PhysicalMemory", ToMegabytes((ulong) pc.RawValue));
					}
				}

				Log("System_CurrentCulture", Thread.CurrentThread.CurrentCulture.Name);
				Log("System_CurrentUiCulture", Thread.CurrentThread.CurrentUICulture.Name);

				Log("System_SystemDefaultEncoding", Encoding.Default.WebName);

				Log("System_CalendarType", CultureInfo.CurrentCulture.Calendar.GetType().Name);

				Log("System_NumberDecimalSeparator", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

				var timeZone = TimeZone.CurrentTimeZone;
				Log("System_TimeZone", timeZone.StandardName);

				var offset = timeZone.GetUtcOffset(DateTime.Now);
				Log("System_TimeZoneOffset", offset.TotalHours);

				var processFile = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName;
				Log("Client_ProcessName", processFile);

				var processVersion = FileVersionInfo.GetVersionInfo(processFile).FileVersion;
				Log("Client_ProcessVersion", processVersion);

				var curAssembly = GetType().Assembly;
				Log("Client_AppMetricsVersion", curAssembly.FullName);
			}
			catch (Exception exc)
			{
				Log(exc);
			}
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
			try
			{
				var curProcess = Process.GetCurrentProcess();

				var workingSet = ToMegabytes((ulong)curProcess.WorkingSet64);
				Log("Client_WorkingSet", workingSet);

				var privateMemorySize = ToMegabytes((ulong)curProcess.PrivateMemorySize64);
				Log("Client_PrivateMemorySize", privateMemorySize);

				var computerInfo = new ComputerInfo();

				Log("System_AvailablePhysicalMemory", ToMegabytes(computerInfo.AvailablePhysicalMemory));
				Log("System_AvailableVirtualMemory", ToMegabytes(computerInfo.AvailableVirtualMemory));

				var processorSecondsUsed = curProcess.TotalProcessorTime.TotalSeconds;
				if (_lastProcessorTimeUsage != 0)
				{
					var secondsPassed = (DateTime.UtcNow - _lastSentPeriodic).TotalSeconds;
					var averageProcessorUsage = ((processorSecondsUsed - _lastProcessorTimeUsage) / secondsPassed) * 100;
					Log("Client_PeriodProcessorUsage", averageProcessorUsage);
				}
				_lastProcessorTimeUsage = processorSecondsUsed;
			}
			catch (Exception exc)
			{
				Log(exc);
			}
		}

		static ulong ToMegabytes(ulong val)
		{
			return val / (1024 * 1024);
		}

		static string Escape(string val)
		{
			var res = val.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
			return res;
		}

		static bool IsUnderMono
		{
			get
			{
				var t = Type.GetType("Mono.Runtime");
				return (t != null);
			}
		}

		public string Url { get; private set; }
		public string ApplicationKey { get; private set; }
		public string SessionId { get; private set; }
		public string AccessKey { get; private set; }

		private static readonly object Sync = new object();
		private static readonly object SendingSync = new object();

		private readonly List<MessageInfo> _messages = new List<MessageInfo>(MaxMessagesCount);
		private readonly StringBuilder _packet = new StringBuilder(MaxPacketSize);

		private static readonly HashSet<Tracker> Sessions = new HashSet<Tracker>();
		private static readonly Thread LoggingThread = new Thread(LoggingThreadEntry);

		private const int MaxMessagesCount = 4096;
		private const int MaxPacketSize = 1024 * 16;
		private static readonly TimeSpan SendingPeriod = TimeSpan.FromSeconds(0.5);

		private static DateTime _lastSentPeriodic;
		private static readonly TimeSpan PeriodicTime = TimeSpan.FromMinutes(10);

		private static double _lastProcessorTimeUsage;

		private static long _requestsSent;
		private static volatile bool _terminated;
	}
}
