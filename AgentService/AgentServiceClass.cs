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

					EnsurePluginsStopped();

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
				Init();
				FindPlugins();
				EnsurePluginsStopped();

				while (!_terminated)
				{
					try
					{
						ApplyUpdates();
						EnsurePluginsStarted();
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

		private static void Init()
		{
			if (!Directory.Exists(Const.WorkingAreaBinPath))
				Directory.CreateDirectory(Const.WorkingAreaBinPath);
			if (!Directory.Exists(Const.WorkingAreaTempPath))
				Directory.CreateDirectory(Const.WorkingAreaTempPath);
		}

		void FindPlugins()
		{
			foreach (var directory in Directory.GetDirectories(Const.WorkingAreaBinPath))
			{
				var pluginName = Path.GetFileName(directory);
				RegisterPlugin(pluginName);
			}
		}

		void ApplyUpdates()
		{
			var settings = AppSettings.Load();

			using (var client = new WebClient())
			{
				client.Credentials = new NetworkCredential(settings.UserName, settings.Password);

				var pluginsList = client.DownloadString(settings.PluginsListUrl);
				var lines = pluginsList.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var line in lines)
				{
					var parts = line.Split(' ');
					if (parts.Count() != 2)
						throw new ApplicationException();
					var name = parts[0];
					var version = parts[1];

					RegisterPlugin(name);
					UpdatePlugin(client, settings.PluginsUrl, name, version);
				}
			}
		}

		private void UpdatePlugin(WebClient client, string pluginsUrl, string name, string newVersion)
		{
			var pluginPath = Const.WorkingAreaBinPath + @"\" + name + @"\";
			if (!Directory.Exists(pluginPath))
				Directory.CreateDirectory(pluginPath);

			// check if update is needed
			var localVersionFile = pluginPath + "version.txt";
			if (File.Exists(localVersionFile))
			{
				var localVersion = File.ReadAllText(localVersionFile);
				if (newVersion == localVersion)
					return;
				ReportEvent(string.Format("Trying to update plugin {0} to version {1}", name, newVersion));
			}

			string zipFileName = name + ".zip";
			var zipFilePath = Const.WorkingAreaTempPath + zipFileName;
			if (File.Exists(zipFilePath))
				File.Delete(zipFilePath);

			var zipFileUrl = pluginsUrl + (name + "/" + zipFileName).Replace("//", "/");
			client.DownloadFile(zipFileUrl, zipFilePath);

			StopPlugin(name);
			DeleteAllFiles(pluginPath);

			using (var zipFile = new ZipFile(zipFilePath))
			{
				zipFile.TempFileFolder = Path.GetTempPath();
				zipFile.ExtractAll(pluginPath);
			}

			{
				var localVersion = File.ReadAllText(localVersionFile).Trim();
				ReportEvent(string.Format("Update of plugin {0} to version {1} is successful", name, localVersion));
			}
		}

		private static void DeleteAllFiles(string path)
		{
			foreach (var file in Directory.GetFiles(path))
			{
				File.Delete(file);
			}
		}

		private void EnsurePluginsStarted()
		{
			lock (_pluginsSync)
			{
				foreach (var pair in _plugins)
				{
					EnsurePluginStarted(pair.Value);
				}
			}
		}

		private void EnsurePluginsStopped()
		{
			lock (_pluginsSync)
			{
				foreach (var pair in _plugins)
				{
					StopPlugin(pair.Value);
				}
			}
		}

		private void EnsurePluginStarted(PluginInfo plugin)
		{
			if (plugin.Process != null)
				return;

			var exePath = Const.GetPluginExePath(plugin.Name);
			var startInfo = new ProcessStartInfo(exePath)
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					RedirectStandardInput = true,
				};
			var process = Process.Start(startInfo);
			plugin.Process = process;
		}

		private void StopPlugin(PluginInfo plugin)
		{
			lock (_pluginsSync)
			{
				try
				{
					// send signal to close
					var stopEvent = EventWaitHandle.OpenExisting("AppMetrics_" + plugin.Name);
					stopEvent.Set();
				}
				catch (Exception exc)
				{
					Report(exc);
				}

				var exePath = Const.GetPluginExePath(plugin.Name);
				if (plugin.Process == null)
				{
					// stop any processes left from the previous agent launch, if any. normally this should not happen
					var processName = Path.GetFileNameWithoutExtension(exePath);
					var processes = Process.GetProcessesByName(processName);
					foreach (var process in processes)
					{
						StopProcess(process);
					}
				}
				else
				{
					StopProcess(plugin.Process);
					plugin.Process = null;
				}
			}
		}

		private void StopProcess(Process process)
		{
			try
			{
				if (!process.WaitForExit(3*1000))
					process.Kill();
			}
			catch (Exception exc)
			{
				Report(exc);
			}
		}

		private void StopPlugin(string name)
		{
			lock (_pluginsSync)
			{
				var plugin = _plugins[name];
				StopPlugin(plugin);
			}
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

		private readonly Dictionary<string, PluginInfo> _plugins = new Dictionary<string, PluginInfo>();
		private readonly object _pluginsSync = new object();

		private void RegisterPlugin(string name)
		{
			lock (_pluginsSync)
			{
				PluginInfo pluginInfo;
				if (!_plugins.TryGetValue(name, out pluginInfo))
					_plugins.Add(name, new PluginInfo(name));
			}
		}

		private readonly object _sync = new object();
		private Thread _thread;
		private volatile bool _terminated;
		private static readonly TimeSpan AutoUpdateCheckPeriod = TimeSpan.FromMinutes(1);
	}
}
