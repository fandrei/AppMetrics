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

		[Test]
		public void TestPercentile98Calculation()
		{
			{
				var data = new[] { 0M, 1M, 2M, 3M, 4M, 5M, 6M, 7M, 8M, 9M, 10M, 11M, 12M, 13M, 14M, 15M, 16M, 17M, 18M, 19M, 
					20M, 21M, 22M, 23M, 24M, 25M, 26M, 27M, 28M, 29M, 30M, 31M, 32M, 33M, 34M, 35M, 36M, 37M, 38M, 39M, 40M, 
					41M, 42M, 43M, 44M, 45M, 46M, 47M, 48M, 49M, 50M, 51M, 52M, 53M, 54M, 55M, 56M, 57M, 58M, 59M, 60M, 61M, 
					62M, 63M, 64M, 65M, 66M, 67M, 68M, 69M, 70M, 71M, 72M, 73M, 74M, 75M, 76M, 77M, 78M, 79M, 80M, 81M, 82M, 
					83M, 84M, 85M, 86M, 87M, 88M, 89M, 90M, 91M, 92M, 93M, 94M, 95M, 96M, 97M, 98M, 99M, };

				var res = Stats.CalculatePercentile98Info(data);

				Assert.AreEqual(2, res.OutliersCount);
				Assert.AreEqual(48.5M, res.Average);
			}
		}
	}
}
