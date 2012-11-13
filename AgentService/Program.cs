using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace AppMetrics.AgentService
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				var arg = args[0];

				try
				{
					if (arg == "-debug")
					{
						var service = new AgentServiceClass();
						service.Start();
						Thread.Sleep(Timeout.Infinite);
					}
					else if (arg == "-install")
					{
						InitWorkingArea();

						if (GetServiceStatus() == null)
						{
							ManagedInstallerClass.InstallHelper(new[] {ExePath, "/LogFile="});
							SetRecoveryOptions(Const.AppName);
						}
					}
					else if (arg == "-uninstall")
					{
						if (GetServiceStatus() != null)
						{
							ManagedInstallerClass.InstallHelper(new[] { "/u", ExePath, "/LogFile=" });
						}
					}
					else if (arg == "-start")
					{
						StartService();
					}
					else if (arg == "-config")
					{
						SetConfig(args.Skip(1));
					}
				}
				catch (Exception exc)
				{
					var message = exc.ToString();
					ShowMessage(message);
				}
			}
			else
			{
				var servicesToRun = new ServiceBase[]
					{
						new AgentServiceClass()
					};
				ServiceBase.Run(servicesToRun);
			}
		}

		private static void StartService()
		{
			using (var controller = new ServiceController(Const.AppName))
			{
				if (controller.Status != ServiceControllerStatus.Running)
				{
					controller.Start();
				}
			}
		}

		private static ServiceControllerStatus? GetServiceStatus()
		{
			try
			{
				var sc = new ServiceController { ServiceName = Const.AppName };
				return sc.Status;
			}
			catch (InvalidOperationException)
			{
				return null;
			}
		}

		static void InitWorkingArea()
		{
			EnsureFolderExists(Const.WorkingAreaPath);

			var accessRights = Directory.GetAccessControl(Const.WorkingAreaPath);
			var accessRule = new FileSystemAccessRule("NETWORK SERVICE", FileSystemRights.FullControl,
				InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None,
				AccessControlType.Allow);
			accessRights.AddAccessRule(accessRule);
			Directory.SetAccessControl(Const.WorkingAreaPath, accessRights);

			EnsureFolderExists(Const.WorkingAreaBinPath);
			EnsureFolderExists(Const.WorkingAreaTempPath);
		}

		private static void EnsureFolderExists(string path)
		{
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
		}

		static void SetConfig(IEnumerable<string> args)
		{
			var argsDic = new Dictionary<string, string>();
			foreach (var arg in args)
			{
				if (arg.StartsWith("-") || arg.StartsWith("/"))
				{
					var tmp = arg.Substring(1);
					if (!tmp.Contains(":"))
					{
						argsDic.Add(tmp.ToLower(), "");
					}
					else
					{
						var parts = tmp.Split(':');
						var value = parts[1];
						if (parts.Count() > 2)
						{
							value = string.Join(":", parts.Skip(1));
						}

						argsDic.Add(parts[0].ToLower(), value);
					}
				}
			}

			if (argsDic.Count == 0)
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new ConfigForm());
				return;
			}

			var settings = AppSettings.Load();

			string userName;
			argsDic.TryGetValue("username", out userName);

			string password;
			argsDic.TryGetValue("password", out password);

			if (!string.IsNullOrEmpty(userName))
			{
				settings.UserName = userName;
				settings.Password = password;

				settings.Save();
			}

			string nodeName;
			argsDic.TryGetValue("nodename", out nodeName);
			if (!string.IsNullOrEmpty(nodeName))
			{
				settings.NodeName = nodeName;

				settings.Save();
			}

			string configServer;
			argsDic.TryGetValue("configserver", out configServer);
			if (!string.IsNullOrEmpty(configServer))
			{
				settings.ConfigBaseUrl = configServer;

				settings.Save();
			}
		}

		static void ShowMessage(string message)
		{
			MessageBox(IntPtr.Zero, message, Const.AppName, MessageBoxOptions.OkOnly | MessageBoxOptions.Topmost);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int MessageBox(IntPtr hWnd, string text, string caption, MessageBoxOptions type);

		[Flags]
		public enum MessageBoxOptions : uint
		{
			OkOnly = 0x000000,
			Topmost = 0x040000
		}

		// set windows service options to restart after failure
		static void SetRecoveryOptions(string serviceName)
		{
			int exitCode;
			using (var process = new Process())
			{
				var startInfo = process.StartInfo;
				startInfo.FileName = "sc";
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;

				startInfo.Arguments = string.Format("failure \"{0}\" reset= 0 actions= restart/60000", serviceName);

				process.Start();
				process.WaitForExit();

				exitCode = process.ExitCode;

				process.Close();
			}

			if (exitCode != 0)
				throw new InvalidOperationException();
		}

		private static readonly string ExePath = Assembly.GetExecutingAssembly().Location;
	}
}
