using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Tests.Functional
{
    [TestFixture]
    public class SystemTimerTests
    {
        /* LON-WS00872 - David's CityIndex work desktop - i7, Win 7 
         *   Operations timed using the system's high-resolution performance counter.
                Timer frequency in ticks per second = 3312822
                Timer is accurate within 301 nanoseconds
         */
        [Test]
        public void DisplayTimerProperties()
        {
            // Display the timer frequency and resolution.
            if (Stopwatch.IsHighResolution)
            {
                Console.WriteLine("Operations timed using the system's high-resolution performance counter.");
            }
            else
            {
                Console.WriteLine("Operations timed using the DateTime class.");
            }

            long frequency = Stopwatch.Frequency;
            Console.WriteLine("  Timer frequency in ticks per second = {0}",
                frequency);
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;
            Console.WriteLine("  Timer is accurate within {0} nanoseconds",
                nanosecPerTick);
        }

    }
}
