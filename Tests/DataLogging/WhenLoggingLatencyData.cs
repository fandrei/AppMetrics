using System;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class WhenLoggingLatencyData : IntegrationTestsBase
    {
        [TestFixtureSetUp]
        public void LogSomeLatencyData()
        {
            Console.WriteLine("LogSomeLatencyData");
           
                
        }
        [Test]
        public void ShouldDoIt()
        {
            Console.WriteLine("ShouldDoIt");
        }

        [Test]
        public void ShouldDoIt2()
        {
            Console.WriteLine("ShouldDoIt");
        }
    }
}