using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using AppMetrics.Analytics;

namespace Tests.Analytics
{
	[TestFixture]
	class UnitTesting
	{
		[Test]
		public void TestQuantilesCalculation()
		{
			var data1 = new decimal[] { 6, 47, 49, 15, 42, 41, 7, 39, 43, 40, 36 };
			var res1 = Stats.CalculateSummaries(data1);

			Assert.IsTrue(res1.LowerQuartile == 25.5M);
			Assert.IsTrue(res1.Median == 40M);
			Assert.IsTrue(res1.UpperQuartile == 42.5M);

			var data2 = new decimal[] { 7, 15, 36, 39, 40, 41 };
			var res2 = Stats.CalculateSummaries(data2);

			Assert.IsTrue(res2.LowerQuartile == 15M);
			Assert.IsTrue(res2.Median == 37.5M);
			Assert.IsTrue(res2.UpperQuartile == 40M);
		}
	}
}
