using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace AppMetrics.Analytics
{
	/// <summary>
	/// Creates reports and delivers to the user
	/// </summary>
	public class GetReport : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			InitProcessing();

			context.Response.ContentType = "text/plain";
			lock (Sync)
			{
				context.Response.Write(_reportText);
			}
		}

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}

		static void InitProcessing()
		{
			lock (Sync)
			{
				if (_workerThread == null)
				{
					_workerThread = new Thread(ThreadStart);
					_workerThread.Start();
				}
			}
		}

		static void ThreadStart()
		{
			while (true)
			{
				try
				{
					CreateReport();
					Thread.Sleep(100);
				}
				catch (ThreadAbortException)
				{
					break;
				}
				catch (Exception exc)
				{
					Trace.WriteLine(exc);
				}
			}
		}

		static void CreateReport()
		{
			
		}

		private static Thread _workerThread;
		private static readonly object Sync = new object();
		private static string _reportText = "";
	}
}