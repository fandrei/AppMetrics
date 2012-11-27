using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using AppMetrics.AgentService.PluginBase;

namespace AppMetrics.AgentService
{
	public partial class ConfigForm : Form
	{
		public ConfigForm()
		{
			InitializeComponent();
			Text = Const.AppName;
		}

		protected override void OnLoad(EventArgs e)
		{
			try
			{
				base.OnLoad(e);

				var settings = AppSettings.Load();

				ConfigServerEdit.Text = settings.ConfigBaseUrl;

				UserNameEdit.Text = settings.UserName;
				PasswordEdit.Text = settings.Password;

				NodeNameEdit.Text = settings.NodeName;
			}
			catch (Exception exc)
			{
				MessageBox.Show(this, exc.ToString(), Const.AppName);
			}
		}

		private void OkButton_Click(object sender, EventArgs e)
		{
			try
			{
				var settings = AppSettings.Load();

				settings.ConfigBaseUrl = ConfigServerEdit.Text;

				settings.UserName = UserNameEdit.Text;
				settings.Password = PasswordEdit.Text;

				settings.NodeName = NodeNameEdit.Text;

				settings.Save();

				Close();
			}
			catch (Exception exc)
			{
				MessageBox.Show(this, exc.ToString(), Const.AppName);
			}
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
