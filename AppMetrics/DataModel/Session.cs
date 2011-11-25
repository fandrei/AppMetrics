using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;
using System.Web;

namespace AppMetrics.DataModel
{
	[DataServiceKey("Id")]
	public class Session
	{
		public string Id { get; set; }
		public DateTime LastUpdated { get; set; }

		internal string FileName { get; set; }
	}
}