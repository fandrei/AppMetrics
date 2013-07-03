using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AppMetrics.Client
{
	static class SafeNativeMethods
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public class MemoryStatusEx
		{
			public uint dwLength;
			public uint dwMemoryLoad;
			public ulong ullTotalPhys;
			public ulong ullAvailPhys;
			public ulong ullTotalPageFile;
			public ulong ullAvailPageFile;
			public ulong ullTotalVirtual;
			public ulong ullAvailVirtual;
			public ulong ullAvailExtendedVirtual;

			public MemoryStatusEx()
			{
				this.dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));
			}
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx lpBuffer);

		public static MemoryStatusEx GetMemoryStatus()
		{
			var res = new MemoryStatusEx();
			if (!GlobalMemoryStatusEx(res))
			{
				throw new Win32Exception();
			}
			return res;
		}
	}
}
