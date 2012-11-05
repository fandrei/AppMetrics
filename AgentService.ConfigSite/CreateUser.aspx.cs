using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AppMetrics.WebUtils;

namespace AppMetrics.AgentService.ConfigSite
{
	public partial class CreateUser : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			try
			{
				WebUtil.CheckIpAddress();
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

				var response = HttpContext.Current.Response;

				if (string.IsNullOrWhiteSpace(UserNameEdit.Text) || string.IsNullOrWhiteSpace(PasswordEdit.Text))
				{
					response.Write("Invalid credentials");
					response.End();
				}

				BasicAuthenticationModule.CreateUser(UserNameEdit.Text, PasswordEdit.Text);

				response.Write("OK");
				response.End();
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
	}
}