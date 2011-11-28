using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppMetrics.Client;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class SmokeTesting
	{
		[Test]
		public void SmokeTest()
		{
			var tracker = new Tracker("http://184.73.228.71/AppMetrics/LogEvent.ashx", "SmokeTest");
			tracker.Log("TestMessage", "TestValue");

			Tracker.Terminate(true);
		}
	}
}
