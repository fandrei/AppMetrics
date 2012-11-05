using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;

using Ionic.Zip;

namespace AppMetrics.AgentService
{
	public partial class AgentServiceClass : ServiceBase
	{
		public AgentServiceClass()
		{
			InitializeComponent();
		}

		public void Start()
		{
			OnStart(new string[0]);
		}

		protected override void OnStart(string[] args)
		{
			lock (_sync)
			{
				_terminated = false;
				_thread = new Thread(ThreadProc);
				_thread.Start();
			}
		}

		protected override void OnStop()
		{
			lock (_sync)
			{
				try
				{
					_terminated = true;
					_thread.Abort();

					StopWorkerDomain();

					_thread.Join();
					_thread = null;
				}
				catch (Exception exc)
				{
					Report(exc);
				}
			}
		}

		void ThreadProc()
		{
			try
			{
				while (!_terminated)
				{
					try
					{
						ApplyUpdates();
						EnsureWorkerDomainStarted();
					}
					catch (ThreadAbortException)
					{
						break;
					}
					catch (Exception exc)
					{
						Report(exc);
					}

					Thread.Sleep(AutoUpdateCheckPeriod);
				}
			}
			catch (ThreadAbortException)
			{
			}
		}

		void ApplyUpdates()
		{
			if (!Directory.Exists(Const.WorkingAreaBinPath))
				Directory.CreateDirectory(Const.WorkingAreaBinPath);
			if (!Directory.Exists(Const.WorkingAreaTempPath))
				Directory.CreateDirectory(Const.WorkingAreaTempPath);

			var settings = AppSettings.Load();
			var updateUrl = settings.AutoUpdateUrl;

			using (var client = new WebClient())
			{
				// compare versions
				var localVersionFile = Const.WorkingAreaBinPath + "version.txt";
				if (File.Exists(localVersionFile))
				{
					var localVersion = File.ReadAllText(localVersionFile);
					var newVersion = client.DownloadString(updateUrl + "version.txt");
					if (newVersion == localVersion)
						return;
					ReportEvent(string.Format("Trying to update to version {0}", newVersion));
				}

				const string zipFileName = "LatencyCollectorCore.zip";
				var zipFilePath = Const.WorkingAreaTempPath + zipFileName;
				if (File.Exists(zipFilePath))
					File.Delete(zipFilePath);
				client.DownloadFile(updateUrl + zipFileName, zipFilePath);

				using (var zipFile = new ZipFile(zipFilePath))
				{
					StopWorkerDomain();
					DeleteAllFiles(Const.WorkingAreaBinPath);

					zipFile.ExtractAll(Const.WorkingAreaBinPath);
				}

				{
					var localVersion = File.ReadAllText(localVersionFile).Trim();
					ReportEvent(string.Format("Update to version {0} is successful", localVersion));
				}
			}
		}

		private static void DeleteAllFiles(string path)
		{
			foreach (var file in Directory.GetFiles(path))
			{
				File.Delete(file);
			}
		}

		private void StartWorkerDomain()
		{
			var newDomain = CreateAppDomain(Const.WorkingAreaBinPath, Const.WorkerAssemblyPath + ".config");
			try
			{
				InvokeCrossDomain(newDomain, "Start");
			}
			catch (AppDomainUnloadedException exc)
			{
				Report(exc);
				newDomain = null;
			}
			catch (Exception exc)
			{
				Report(exc);
				AppDomain.Unload(newDomain);
				throw;
			}

			_appDomain = newDomain;

			ReportEvent("Started collecting");
		}

		void StopWorkerDomain()
		{
			if (_appDomain == null)
				return;

			InvokeCrossDomain(_appDomain, "Stop");
			AppDomain.Unload(_appDomain);
			_appDomain = null;
		}

		static AppDomain CreateAppDomain(string basePath, string configPath)
		{
			var setup = new AppDomainSetup
				{
					ApplicationBase = basePath,
					ConfigurationFile = configPath,
				};
			var appDomain = AppDomain.CreateDomain("LatencyCollectorCore", null, setup);
			return appDomain;
		}

		private void EnsureWorkerDomainStarted()
		{
			if (_appDomain != null)
			{
				try
				{
					var tmp = _appDomain.Id;
				}
				catch (AppDomainUnloadedException exc)
				{
					Report(exc);
					_appDomain = null;
				}
			}

			if (_appDomain == null)
				StartWorkerDomain();
		}

		private static void InvokeCrossDomain(AppDomain newDomain, string methodName)
		{
			var args = new object[] { "LatencyCollectorCore.Program", methodName, null };
			newDomain.CreateInstanceFromAndUnwrap(Const.WorkerAssemblyPath,
				"LatencyCollectorCore.Proxy", false, BindingFlags.Default, null, args, CultureInfo.InvariantCulture, null);
		}

		public static void ReportEvent(string message, EventLogEntryType type = EventLogEntryType.Information)
		{
			Trace.WriteLine(message);

			try
			{
				EventLog.WriteEntry(Const.AppName, message, type);
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
			}
		}

		public static void Report(Exception exc)
		{
			ReportEvent(exc.ToString(), EventLogEntryType.Warning);
		}

		private readonly object _sync = new object();
		private Thread _thread;
		private volatile bool _terminated;
		private AppDomain _appDomain;
		private static readonly TimeSpan AutoUpdateCheckPeriod = TimeSpan.FromMinutes(1);
	}
}
