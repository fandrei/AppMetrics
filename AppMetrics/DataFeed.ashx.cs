using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;

namespace AppMetrics
{
	/// <summary>
	/// Summary description for DataFeed
	/// </summary>
	public class DataFeed : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			try
			{
				var items = new List<SyndicationItem>();

				var dataPath = Util.GetDataFolderPath();
				foreach (var file in Directory.GetFiles(dataPath, "*.*.txt", SearchOption.AllDirectories))
				{
					if (file.EndsWith(Const.LogFileName, StringComparison.OrdinalIgnoreCase))
						continue;

					var name = file.Substring(dataPath.Length + 1);
					var text = File.ReadAllText(file);
					var fileTime = File.GetLastWriteTime(file);
					var item = new SyndicationItem(name, text, null, name, fileTime);
					items.Add(item);
				}
				items.Sort((x, y) => x.LastUpdatedTime.CompareTo(y.LastUpdatedTime));

				var feed = new SyndicationFeed(Const.AppName, "", null, items);
				using (var writer = XmlWriter.Create(context.Response.Output, null))
				{
					var formatter = new Atom10FeedFormatter(feed);
					formatter.WriteTo(writer);
				}
			}
			catch (Exception exc)
			{
				Trace.WriteLine(exc);
			}
		}

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}
	}
}