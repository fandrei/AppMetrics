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

		[Test]
		public void TestDistributionCalculation()
		{
			{
				var data = new[] { 0M, 0.1M, 0.6M, 0.8M, 1.1M, 2.2M, 2.4M, };
				var distribution = Stats.CalculateDistribution(data, 0.5M);
				var sample = new SortedDictionary<decimal, int>
				    {
						{ 0, 1 },
				        { 0.5M, 1 },
						{ 1, 2 },
						{ 1.5M, 1 },
						{ 2.5M, 2 }
				    };
				Assert.IsTrue(distribution.Vals.SequenceEqual(sample));
			}

			{
				var data = new[] { 0M, 0.1M, 0.2M, 0.21M, 0.25M };
				var distribution = Stats.CalculateDistribution(data, 0.2M);
				var sample = new SortedDictionary<decimal, int>
				    {
				        { 0, 1 },
						{ 0.2M, 2 },
						{ 0.4M, 2 },
				    };
				Assert.IsTrue(distribution.Vals.SequenceEqual(sample));
			}
		}
	}
}
