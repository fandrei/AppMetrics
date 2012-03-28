using System;
using System.Collections.Generic;
using System.Linq;

namespace AppMetrics
{
	public partial class Config : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			LogEvent.Init();

			if (!Page.IsPostBack)
			{
				AccessKeyEdit.Text = AppSettings.Instance.AmazonAccessKey;
				SecretAccessKeyEdit.Text = AppSettings.Instance.AmazonSecretAccessKey;
			}
		}

		protected void OkButton_Click(object sender, EventArgs e)
		{
			AppSettings.Instance.AmazonAccessKey = AccessKeyEdit.Text;
			AppSettings.Instance.AmazonSecretAccessKey = SecretAccessKeyEdit.Text;
			AppSettings.Instance.Save();
		}
	}
}