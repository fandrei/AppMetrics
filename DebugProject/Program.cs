using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace DebugProject
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var url = "http://localhost:51379/LogEvent.ashx";
				var vals = new NameValueCollection { { "Session", Guid.NewGuid().ToString() }, { "Data", DateTime.Now.ToString() } };
				using (var client = new WebClient())
				{
					var response = client.UploadValues(url, "POST", vals);
					var responseText = Encoding.ASCII.GetString(response);
					Console.WriteLine(responseText);
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc);
			}
		}
	}
}
