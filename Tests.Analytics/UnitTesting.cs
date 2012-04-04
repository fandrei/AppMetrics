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
	    /// <summary>
		/// http://en.wikipedia.org/wiki/Percentile : 
		/// In statistics, a percentile (or centile) is the value of a variable below which 
		/// a certain percent of observations fall. For example, the 20th percentile is the 
		/// value (or score) below which 20 percent of the observations may be found. 
		/// The term percentile and the related term percentile rank are often used in the 
		/// reporting of scores from norm-referenced tests.
		/// 
		/// The 25th percentile is also known as the first quartile (Q1), the 50th percentile 
		/// as the median or second quartile (Q2), and the 75th percentile as the third quartile (Q3).
		/// 
		/// R gives the following:
		/// > data1 = c(6, 47, 49, 15, 42, 41, 7, 39, 43, 40, 36);
		/// > data2 = c(7, 15, 36, 39, 40, 41);
		/// 
		/// > quantile(data1, c(.25, .50, .75));
		///    25%  50%  75% 
		///   25.5 40.0 42.5
		/// > quantile(data2, c(.25, .50, .75));
		///    25%   50%   75% 
		///   20.25 37.50 39.75 
		/// </summary>
		[Test]
		public void TestSummariesCalculation()
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

				Assert.IsTrue(res.LowerQuartile == 20.25M);
				Assert.IsTrue(res.Median == 37.5M);
				Assert.IsTrue(res.UpperQuartile == 39.75M);
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

        private readonly decimal[] _sampleData = new[] { 0M, 1M, 2M, 3M, 4M, 5M, 6M, 7M, 8M, 9M, 10M, 11M, 12M, 13M, 14M, 15M, 16M, 17M, 18M, 19M, 
                                  20M, 21M, 22M, 23M, 24M, 25M, 26M, 27M, 28M, 29M, 30M, 31M, 32M, 33M, 34M, 35M, 36M, 37M, 38M, 39M, 40M, 
                                  41M, 42M, 43M, 44M, 45M, 46M, 47M, 48M, 49M, 50M, 51M, 52M, 53M, 54M, 55M, 56M, 57M, 58M, 59M, 60M, 61M, 
                                  62M, 63M, 64M, 65M, 66M, 67M, 68M, 69M, 70M, 71M, 72M, 73M, 74M, 75M, 76M, 77M, 78M, 79M, 80M, 81M, 82M, 
                                  83M, 84M, 85M, 86M, 87M, 88M, 89M, 90M, 91M, 92M, 93M, 94M, 95M, 96M, 97M, 98M, 99M, };
		/// <summary>
		/// http://en.wikipedia.org/wiki/Percentile
		/// http://en.wikipedia.org/wiki/Quantile
		/// In statistics, a percentile (or centile) is the value of a variable below which 
		/// a certain percent of observations fall. For example, the 20th percentile is the 
		/// value (or score) below which 20 percent of the observations may be found. 
		/// The term percentile and the related term percentile rank are often used in the 
		/// reporting of scores from norm-referenced tests.
		/// 
		/// R gives the following:
		/// > data = c(0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,
		///     20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,
		///     41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,
		///     62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,
		///     83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99);
		/// 
		/// > quantile(data, c(.02, .25, .50, .75, .98));
		///       2%   25%   50%   75%   98% 
		///      1.98 24.75 49.50 74.25 97.02 
		/// 
		/// It's important that there are different algorithms for percentile and quantile calculation,
		/// giving different results; however, these results are very similar when calculating on big amount of data
		/// we use the same algorithm as used in R by default (using linear interpolation)
		/// </summary>
		[Test]
		public void TestPercentileCalculation()
		{
			var quantileSpecs = new[] { 0.02M, 0.25M, 0.5M, 0.75M, 0.98M };
            var quantiles = Stats.CalculateQuantiles(_sampleData, quantileSpecs);

			var quantilesSample = new[] { 1.98M, 24.75M, 49.50M, 74.25M, 97.02M, };
			Assert.IsTrue(quantiles.SequenceEqual(quantilesSample));

            var indexes = Stats.CalculateQuantileIndexes(_sampleData, quantileSpecs);
			var indexesSample = new[] { 1.98M, 24.75M, 49.50M, 74.25M, 97.02M, }; // on these data indexes are equal to quantiles
			Assert.IsTrue(indexes.SequenceEqual(indexesSample));
		}

        [Test]
        public void SummaryShouldIncludeMinAndMax()
        {
            var res = Stats.CalculateSummaries(_sampleData);

            Assert.IsTrue(res.Min == 0M);
            Assert.IsTrue(res.Max == 99M);
        }

        [Test]
        public void SummaryShouldIncludeMedian()
        {
            var res = Stats.CalculateSummaries(_sampleData);

            Assert.IsTrue(res.Median == 49.5M);
        }

        [Test]
        public void SummaryShouldIncludeAverage()
        {
            var res = Stats.CalculateSummaries(_sampleData);

            Assert.IsTrue(res.Average == 49.5M);
        }

        [Test]
        public void SummaryShouldIncludeLowerAndUpperQuartile()
        {
            var res = Stats.CalculateSummaries(_sampleData);

            Assert.IsTrue(res.Min == 0M);
            Assert.IsTrue(res.Max == 99M);
        }

        [Test]
        public void SummaryShouldInclude2And98Percentile()
        {
            var res = Stats.CalculateSummaries(_sampleData);

			Assert.IsTrue(res.Percentile2 == 1.98M);
			Assert.IsTrue(res.Percentile98 == 97.02M);
        }


	}
}
