using System;
using System.Collections.Generic;
using System.Linq;

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
				LogEvent.Init();

				if (!Page.IsPostBack)
				{
					AccessKeyEdit.Text = AppSettings.Instance.AmazonAccessKey;
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

				AppSettings.Instance.AmazonAccessKey = AccessKeyEdit.Text;
				AppSettings.Instance.AmazonSecretAccessKey = SecretAccessKeyEdit.Text;
				AppSettings.Instance.Save();
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
	}
}