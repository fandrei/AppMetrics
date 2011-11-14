using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace AppMetrics.Client
{
	public class Tracker : IDisposable
	{
		public Tracker(string url)
		{
			_url = url;
			_session = Guid.NewGuid().ToString();
		}

		public void Log(string name, object val)
		{
			Log(name, val, 0);
		}

		public void Log(string name, object val, int index)
		{
			var paramName = "TrackerData" + index;
			var vals = new NameValueCollection { { "TrackerSession", _session }, { paramName, name + "\r\n" + val } };
			using (var client = new WebClient())
			{
				var response = client.UploadValues(_url, "POST", vals);
				var responseText = Encoding.ASCII.GetString(response);
				if (!string.IsNullOrEmpty(responseText))
					throw new ApplicationException(responseText);
			}
		}

		private readonly WebClient _client = new WebClient();
		private readonly string _session;
		private readonly string _url;

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}
