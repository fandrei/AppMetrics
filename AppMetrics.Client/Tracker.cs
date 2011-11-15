using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

namespace AppMetrics.Client
{
	public class Tracker
	{
		public Tracker(string url)
		{
			if (string.IsNullOrEmpty(url))
				throw new ArgumentNullException();
			_url = url;
			_session = Guid.NewGuid().ToString();
		}

		static Tracker()
		{
			LoggingThread.Start();
		}

		public static void Terminate(bool waitAll = false)
		{
			_terminated = true;
			LoggingThread.Interrupt();
			var period = waitAll ? Timeout.Infinite : 5 * 1000;
			LoggingThread.Join(period);
		}

		public void Log(string name, object val, MessageSeverity severity = MessageSeverity.Low)
		{
			lock (Sync)
			{
				if (Messages.Count >= MaxMessagesCount)
				{
					Messages.RemoveAll(message => message.Severity == MessageSeverity.Low);
					AddMessage(WarningName, "Message queue overflow. Some messages are skipped.", MessageSeverity.High);
					if (Messages.Count >= MaxMessagesCount) // too much high-priority messages
					{
						Messages.Clear();
						AddMessage(ErrorName, "Critical message queue overflow. All messages are removed.", MessageSeverity.High);
					}
					SendMessages(); // send warning message immediately
				}

				AddMessage(name, val, severity);
			}
		}

		private const string WarningName = "AppMetrics.Warning";
		private const string ErrorName = "AppMetrics.Error";

		private void AddMessage(string name, object val, MessageSeverity severity)
		{
			lock (Sync)
			{
				Messages.Add(
					new MessageInfo
						{
							Name = name,
							Value = val.ToString(),
							SessionId = _session,
							Url = _url,
							Time = DateTime.UtcNow,
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
			}
			catch (ThreadInterruptedException)
			{ }

			SendMessages();
		}

		private static void SendMessages()
		{
			try
			{
				List<MessageInfo> messages;
				lock (Sync)
				{
					messages = new List<MessageInfo>(Messages);
					Messages.Clear();
				}

				using (var client = new WebClient())
				{
					foreach (var message in messages)
					{
						SendMessage(client, message);
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
					{ "MessageSession", message.SessionId }, 
					{ "MessageName", message.Name },
					{ "MessageData", message.Value },
					{ "MessageTime", message.Time.ToString("u") },
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

		private readonly string _session;
		private readonly string _url;

		private static readonly object Sync = new object();
		private static readonly List<MessageInfo> Messages = new List<MessageInfo>(MaxMessagesCount);
		private const int MaxMessagesCount = 1024;
		private static readonly Thread LoggingThread = new Thread(LoggingThreadEntry);

		private static long _requestsSent;
		private static volatile bool _terminated;
	}
}
