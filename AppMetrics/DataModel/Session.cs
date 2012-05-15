﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace AppMetrics.DataModel
{
	public class Session
	{
		public string Id { get; set; }

		public DateTime CreationTime { get; set; }
		public DateTime LastUpdateTime { get; set; }

		public string FileName;

		public override string ToString()
		{
			var res = string.Format("{0} | {1}", CreationTime, LastUpdateTime);
			return res;
		}

		public string Serialize()
		{
			var res = string.Format("{0}\t{1}\t{2}", Id, CreationTime, LastUpdateTime);
			return res;
		}
	}
}