using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.Client
{
	public abstract class TrackerBase : IDisposable
	{
		public abstract void Dispose();
	}
}
