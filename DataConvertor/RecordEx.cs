﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AppMetrics.DataModel;

namespace AppMetrics.DataConvertor
{
	class RecordEx : Record
	{
		public RecordEx(SessionEx session)
		{
			Session = session;
		}

		public SessionEx Session { get; private set; }
	}
}