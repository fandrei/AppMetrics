using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AppMetrics.Backup.Resources;

namespace AppMetrics.Backup
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length < 1)
					ExitAndShowManual();

				var dataPath = args[0];

				AppSettings.Load(dataPath);

				if (args.Length >= 2)
				{
					if (args[1] == "-config")
					{
						ExitAndShowManual();

						return;
					}
				}

				using (var mutex = new Mutex(false, "AppMetrics.Backup"))
				{
					if (!mutex.WaitOne(0, false))
						throw new ApplicationException("Another instance is running");

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

		static void ExitAndShowManual()
		{
			throw new ApplicationException(Resource.Manual);
		}
	}

	public delegate void ReportLogDelegate(object val, LogPriority priority = LogPriority.High);}
