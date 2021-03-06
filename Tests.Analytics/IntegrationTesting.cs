﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

using AppMetrics.Shared;
using AppMetrics.Analytics;

namespace Tests.Analytics
{
	[TestFixture]
	public class IntegrationTesting
	{
		[Test]
		public void TestInternalValidation()
		{
			var dataPath = Util.GetAppLocation() + @"\Data\";
			var sessions = LogReader.Parse(dataPath, TimePeriod.Unlimited);
			Assert.IsTrue(sessions.Count > 0);

			var options = new AnalysisOptions { ApplicationKey = "CIAPI.CS.Excel" };
			var convertor = new StatsBuilder();
			var res = convertor.Process(sessions, options);
			Assert.IsTrue(res != null);

			var summaryReport = Report.GetSummaryReport(sessions);
			Assert.IsTrue(summaryReport != null);
		}

		[Test]
		public void TestReportsCorrectness()
		{
			var dataPath = Util.GetAppLocation() + @"\Data\";
			var sessions = LogReader.Parse(dataPath, TimePeriod.Unlimited);
			Assert.IsTrue(sessions.Count > 0);

			var options = new AnalysisOptions
				{
					ApplicationKey = "CIAPI.CS.Excel",
					LocationIncludeOverall = true,
				};
			var convertor = new StatsBuilder();
			var res = convertor.Process(sessions, options);
			Assert.IsTrue(res != null);

			var resultsPath = Util.GetAppLocation() + @"\Results\";

			{
				var summaryReport = Report.GetSummaryReport(sessions);
				var summarySample = File.ReadAllText(resultsPath + "TestData.Summary.txt");
				Assert.AreEqual(summaryReport, summarySample);
			}

			{
				var latencySummariesReport = Report.GetLatencyStatSummariesReport(res, options);
				var latencySummariesSample = File.ReadAllText(resultsPath + "TestData.LatencyStatSummaries.txt");
				Assert.AreEqual(latencySummariesSample, latencySummariesReport);
			}

			{
				var latencyDistributionReport = Report.GetLatencyDistributionReport(res);
				var latencyDistributionSample = File.ReadAllText(resultsPath + "TestData.LatencyDistribution.txt");
				Assert.AreEqual(latencyDistributionSample, latencyDistributionReport);
			}

			{
				var jitterDistributionReport = Report.GetJitterDistributionReport(res);
				var jitterDistributionSample = File.ReadAllText(resultsPath + "TestData.JitterDistribution.txt");
				Assert.AreEqual(jitterDistributionSample, jitterDistributionReport);
			}
		}
	}
}
