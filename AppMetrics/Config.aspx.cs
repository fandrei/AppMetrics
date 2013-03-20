using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AppMetrics.WebUtils;

namespace AppMetrics
{
	public partial class Config : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			try
			{
				WebUtil.CheckIpAddress();

				if (!Page.IsPostBack)
				{
					RequireAccessKey.Checked = AppSettings.Instance.RequireAccessKey;

					if (AppSettings.Instance.AccessKeys != null)
					{
						var text = new StringBuilder();
						foreach (var accessKey in AppSettings.Instance.AccessKeys)
						{
							text.AppendLine(accessKey);
						}
						AccessKeysList.Text = text.ToString();
					}
				}
			}
			catch (UnauthorizedAccessException)
			{
			}
		}

		protected void OkButton_Click(object sender, EventArgs e)
		{
			try
			{
				WebUtil.CheckIpAddress();

				AppSettings.Instance.RequireAccessKey = RequireAccessKey.Checked;

				var text = AccessKeysList.Text;
				var keys = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				AppSettings.Instance.AccessKeys = keys;

				AppSettings.Instance.Save();
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
	}
}