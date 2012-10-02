using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace ReportingToAppMetrics
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Test();
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}

		//////////////////////////////////////////////////////////////////////////////
		// LogEvent.ashx is an entry point, to send data it's necessary to request it with appropriate parameters
		// (can use both GET and POST request methods)

		// parameters explanation
		// "MessageAppKey" identifies application that sends data
		// "MessageSession" unique identifier of user's session
		// "MessagesList" contains one or more message to send;
		// every it's line represents one message and consists of three columns delimited by tabs and ending with linefeed
		// these columns are:
		// time when event happened, UTC time zone, in this time format: "yyyy-MM-dd HH:mm:ss.fffffff"
		// name of the message type
		// message data

		// it's better to send multiple messages at once, because every web request can have big round-trip delay

		// samples of MessageName
		// "Event"
		// "Exception"
		// "Latency Login"

		// sample of the message line:
		// 2012-04-02 08:56:16.0527220\tLatency TestMethod\t0.0992977\r\n

		// response is empty on success, or contains error message otherwise
		//////////////////////////////////////////////////////////////////////////////

		static void Test()
		{
			// it's not recommended to use system clock for latency measurement, because it results can be very inaccurate
			// using profiling API class instead
			var watch = Stopwatch.StartNew();
			Thread.Sleep(100);
			watch.Stop();

			var latency = watch.Elapsed.TotalSeconds;

			var message1 = string.Format("{0}\t{1}\t{2}\r\n", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
				"Event", "Test");
			var message2 = string.Format("{0}\t{1}\t{2}\r\n", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
				"Latency TestMethod", latency.ToString(CultureInfo.InvariantCulture));

			var sessionId = Guid.NewGuid().ToString();
			var vals = new NameValueCollection
				{
					{ "MessageAppKey", "Sample_ReportingToAppMetrics" },
					{ "MessageSession", sessionId },
					{ "MessagesList", message1 + message2 },
				};

			// it's better to send data from the secondary thread, but in this sample using only one thread in the sake of simplicity
			using (var client = new WebClient())
			{
				var response = client.UploadValues(ServerUrl, "POST", vals);
				var responseText = Encoding.ASCII.GetString(response);
				if (!string.IsNullOrEmpty(responseText))
					throw new ApplicationException(responseText);
			}
		}

		private const string ServerUrl = "http://metrics.labs.cityindex.com/LogEvent.ashx";
	}
}
