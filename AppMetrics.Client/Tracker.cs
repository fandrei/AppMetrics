using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

namespace AppMetrics.Client
{
	public class Tracker : IDisposable
	{
		public Tracker(string url)
		{
			_url = url;
			_session = Guid.NewGuid().ToString();
		}

		public void Dispose()
		{
			_client.Dispose();
		}

		static Tracker()
		{
			LoggingThread.Start();
		}

		public static void Terminate()
		{
			LoggingThread.Interrupt();
			LoggingThread.Join(TimeSpan.FromSeconds(5));
		}

		public void Log(string name, object val)
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
							Time = DateTime.UtcNow
						});
			}
		}

		static void LoggingThreadEntry()
		{
			try
			{
				_client = new WebClient();

				while (true)
				{
					SendMessages();
					Thread.Sleep(TimeSpan.FromMilliseconds(100));
				}
			}
			catch (ThreadInterruptedException)
			{ }

			SendMessages();
			_client.Dispose();
		}

		private static void SendMessages()
		{
			List<MessageInfo> messages;
			lock (Sync)
			{
				messages = new List<MessageInfo>(Messages);
				Messages.Clear();
			}

			foreach (var message in messages)
			{
				SendMessage(message);
			}
		}

		static void SendMessage(MessageInfo message)
		{
			var vals = new NameValueCollection
				{
					{ "MessageSession", message.SessionId }, 
					{ "MessageName", message.Name },
					{ "MessageData", message.Value },
					{ "MessageTime", message.Time.ToString("u") },
				};

			var response = _client.UploadValues(message.Url, "POST", vals);
			var responseText = Encoding.ASCII.GetString(response);
			if (!string.IsNullOrEmpty(responseText))
				throw new ApplicationException(responseText);
		}

		private readonly string _session;
		private readonly string _url;

		private static WebClient _client;

		private static readonly object Sync = new object();
		private static readonly List<MessageInfo> Messages = new List<MessageInfo>();
		private static readonly Thread LoggingThread = new Thread(LoggingThreadEntry);
	}
}
