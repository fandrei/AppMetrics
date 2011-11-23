using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;
using System.Web;

namespace AppMetrics.DataModel
{
	[DataServiceKey("Time")]
	public class Record
	{
		public DateTime Time { get; set; }
		public string Name { get; set; }
		public string Value { get; set; }
	}
}