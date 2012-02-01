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
			{
				var data = new decimal[] { 6, 47, 49, 15, 42, 41, 7, 39, 43, 40, 36 };
				var res = Stats.CalculateSummaries(data);

				Assert.IsTrue(res.LowerQuartile == 25.5M);
				Assert.IsTrue(res.Median == 40M);
				Assert.IsTrue(res.UpperQuartile == 42.5M);
			}

			{
				var data = new decimal[] { 7, 15, 36, 39, 40, 41 };
				var res = Stats.CalculateSummaries(data);

				Assert.IsTrue(res.LowerQuartile == 15M);
				Assert.IsTrue(res.Median == 37.5M);
				Assert.IsTrue(res.UpperQuartile == 40M);
			}
		}
		}
	}
}
