using System;
using System.Collections.Generic;
using System.Linq;

using AppMetrics.Analytics;

namespace AppMetrics.AnalyticsSite
{
	public class ReportInfo
	{
		public AnalysisOptions Options;

		public string ReportText;

		public DateTime LastUpdateTime;
		public TimeSpan GenerationElapsed;
	}
}