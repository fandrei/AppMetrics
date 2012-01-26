using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using AppMetrics.Analytics;

namespace Tests.Analytics
{
	[TestFixture]
	public class SmokeTesting
	{
		[Test]
		public void SmokeTest()
		{
			var dataPath = Util.GetAppLocation() + @"\Data\";
			var sessions = LogReader.Parse(dataPath, TimeSpan.MaxValue);
			Assert.IsTrue(sessions.Count > 0);

			var convertor = new StatsBuilder();
			var res = convertor.Process(sessions);
			Assert.IsTrue(res != null);

			var summaryReport = Report.GetSummaryReport(sessions);
			Assert.IsTrue(summaryReport != null);
		}
	}
}
