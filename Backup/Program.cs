using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AppMetrics.Backup
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length != 1)
					throw new ApplicationException("Invalid command line arguments");

				var dataPath = args[0];

				using (var mutex = new Mutex(false, "AppMetrics.Backup"))
				{
					if (!mutex.WaitOne(0, false))
						throw new ApplicationException("Another instance is running");

					AppSettings.Load(dataPath);

					BackupHandler.BackupAll(dataPath, 
						(val, priority) => Console.WriteLine(val));
				}
			}
			catch (ApplicationException exc)
			{
				Console.WriteLine(exc.Message);
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}
	}

	public delegate void ReportLogDelegate(object val, LogPriority priority = LogPriority.High);
}
