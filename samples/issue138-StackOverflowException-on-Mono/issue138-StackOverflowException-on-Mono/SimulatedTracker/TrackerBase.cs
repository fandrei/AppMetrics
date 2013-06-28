using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace issue138StackOverflowExceptiononMono
{
	public enum MessagePriority { Low = 0, High }

	public abstract class TrackerBase : IDisposable
	{
		public abstract void Dispose();

		public abstract void Log(string name, string val, MessagePriority priority = MessagePriority.Low);
		public abstract void FlushMessages();

		public void Log(Exception exc)
		{
			Log("Exception", exc.ToString(), MessagePriority.High);
		}

		public void Log(string name, object val, MessagePriority priority = MessagePriority.Low)
		{
			Log(name, val.ToString(), priority);
		}

		public void Log(string name, double val, MessagePriority priority = MessagePriority.Low)
		{
			Log(name, val.ToString(CultureInfo.InvariantCulture), priority);
		}

		public void Log(string name, float val, MessagePriority priority = MessagePriority.Low)
		{
			Log(name, val.ToString(CultureInfo.InvariantCulture), priority);
		}

		public void Log(string name, decimal val, MessagePriority priority = MessagePriority.Low)
		{
			Log(name, val.ToString(CultureInfo.InvariantCulture), priority);
		}

		public void Log(string name, long val, MessagePriority priority = MessagePriority.Low)
		{
			Log(name, val.ToString(CultureInfo.InvariantCulture), priority);
		}

		public void Log(string name, ulong val, MessagePriority priority = MessagePriority.Low)
		{
			Log(name, val.ToString(CultureInfo.InvariantCulture), priority);
		}

		public void LogFormat(string name, MessagePriority priority, string format, params object[] args)
		{
			var text = string.Format(CultureInfo.InvariantCulture, format, args);
			Log(name, text, priority);
		}

		public void LogFormat(string name, string format, params object[] args)
		{
			LogFormat(name, MessagePriority.Low, format, args);
		}

		public Stopwatch StartMeasure()
		{
			return Stopwatch.StartNew();
		}

		public void EndMeasure(Stopwatch watch, string label)
		{
			var diff = watch.Elapsed;
			watch.Stop();

			LogLatency(label, diff.TotalSeconds);
		}

		public void LogLatency(string label, double value)
		{
			Log("Latency " + label, value);
		}
	}
}
