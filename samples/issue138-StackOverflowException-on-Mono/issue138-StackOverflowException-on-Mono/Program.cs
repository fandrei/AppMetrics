using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualBasic.Devices;

namespace issue138StackOverflowExceptiononMono
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			/* When run under Mono, generates evens like:
			 * POST http://foo.com
					AccessKey=accessKey
					MessageAppKey=appKey
					MessagesList=7ca6f023-f945-4052-a566-609aae8acde0	2013-06-27 17:10:02.9872070	System_OsName	Unix
				7ca6f023-f945-4052-a566-609aae8acde0	2013-06-27 17:10:02.9892350	System_OsVersion	Unix 12.4.0.0
				7ca6f023-f945-4052-a566-609aae8acde0	2013-06-27 17:10:02.9893300	System_ComputerName	maccy001
				7ca6f023-f945-4052-a566-609aae8acde0	2013-06-27 17:10:02.9893920	System_UserName	mrdavidlaing
				7ca6f023-f945-4052-a566-609aae8acde0	2013-06-27 17:10:02.9908230	System_ClrVersion	4.0.30319.17020
				7ca6f023-f945-4052-a566-609aae8acde0	2013-06-27 17:10:03.0028930	Exception	System.NotImplementedException: The requested feature is not implemented.\n  at Microsoft.VisualBasic.Devices.ComputerInfo.get_TotalPhysicalMemory () [0x00000] in /private/tmp/source/bockbuild-crypto-mono/profiles/mono-mac-xamarin/build-root/mono-mono-basic-6bb2ca6/vbruntime/Microsoft.VisualBasic/Microsoft.VisualBasic.Devices/ComputerInfo.vb:80 \n  at issue138StackOverflowExceptiononMono.Tracker.ReportSystemInfo () [0x000a0] in /Users/mrdavidlaing/Projects/fandrei/AppMetrics/samples/issue138-StackOverflowException-on-Mono/issue138-StackOverflowException-on-Mono/SimulatedTracker/Tracker.cs:312 
				7ca6f023-f945-4052-a566-609aae8acde0	2013-06-27 17:10:03.0029940	Client_WorkingSet	8
				7ca6f023-f945-4052-a566-609aae8acde0	2013-06-27 17:10:03.0039930	Client_PrivateMemorySize	10231566338
				7ca6f023-f945-4052-a566-609aae8acde0	2013-06-27 17:10:03.0041890	Exception	System.NotImplementedException: The requested feature is not implemented.\n  at Microsoft.VisualBasic.Devices.ComputerInfo.get_AvailablePhysicalMemory () [0x00000] in /private/tmp/source/bockbuild-crypto-mono/profiles/mono-mac-xamarin/build-root/mono-mono-basic-6bb2ca6/vbruntime/Microsoft.VisualBasic/Microsoft.VisualBasic.Devices/ComputerInfo.vb:42 \n  at issue138StackOverflowExceptiononMono.Tracker.ReportPeriodicInfo () [0x00040] in /Users/mrdavidlaing/Projects/fandrei/AppMetrics/samples/issue138-StackOverflowException-on-Mono/issue138-StackOverflowException-on-Mono/SimulatedTracker/Tracker.cs:374 
			 */
			var tracker = Tracker.Create ("http://foo.com", "appKey", "accessKey");

			/* No errors when run under Mono */
//			var tracker = MonoCompatableTracker.Create ("http://foo.com", "appKey", "accessKey");

			var watch = tracker.StartMeasure ();
			tracker.EndMeasure (watch, "Dummy event");

			watch = tracker.StartMeasure ();
			tracker.EndMeasure (watch, "Dummy event 2");


			// Note that HttpUtil has been customised to log messages to the console rather than make HttpRequests
			tracker.FlushMessages ();

			tracker.Dispose ();
		}
	}
}
