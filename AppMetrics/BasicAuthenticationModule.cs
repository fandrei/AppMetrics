using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace AppMetrics
{
	public class BasicAuthenticationModule : IHttpModule
	{
		public void Init(HttpApplication context)
		{
			context.AuthenticateRequest += AuthenticateRequest;
		}

		public void Dispose() { }

		static void AuthenticateRequest(object sender, EventArgs e)
		{
			var application = (HttpApplication)sender;
			if (!Authenticate(application.Context))
			{
				application.Context.Response.Status = "401 Unauthorized";
				application.Context.Response.StatusCode = 401;
				application.Context.Response.AddHeader("WWW-Authenticate", "Basic");
				application.CompleteRequest();
			}
		}

		static bool Authenticate(HttpContext context)
		{
			//if (!context.Request.IsSecureConnection)
			//    return false;

			if (!context.Request.Headers.AllKeys.Contains("Authorization"))
				return false;

			var authHeader = context.Request.Headers["Authorization"];

			IPrincipal principal;
			if (TryGetPrincipal(authHeader, out principal))
			{
				context.User = principal;
				return true;
			}
			return false;
		}

		private static bool TryGetPrincipal(string authHeader, out IPrincipal principal)
		{
			var creds = ParseAuthHeader(authHeader);
			if (creds != null && TryGetPrincipal(creds, out principal))
				return true;

			principal = null;
			return false;
		}

		private static string[] ParseAuthHeader(string authHeader)
		{
			if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic"))
				return null;

			var base64Credentials = authHeader.Substring(6);
			var credentials = Encoding.ASCII.GetString(Convert.FromBase64String(base64Credentials)).Split(new[] { ':' });

			if (credentials.Length != 2 || string.IsNullOrEmpty(credentials[0]) || string.IsNullOrEmpty(credentials[1]))
				return null;

			return credentials;
		}

		private static bool TryGetPrincipal(string[] creds, out IPrincipal principal)
		{
			var users = GetUsers();

			foreach (var pair in users)
			{
				if (creds[0] == pair.Key && creds[1] == pair.Value)
				{
					principal = new GenericPrincipal(new GenericIdentity(pair.Key), new[] { "Administrator", "User" });
					return true;
				}
			}

			principal = null;
			return false;
		}

		private static Dictionary<string, string> _users;
		private static readonly object Sync = new object();

		static Dictionary<string, string> GetUsers()
		{
			lock (Sync)
			{
				try
				{
					if (_users == null)
					{
						var fileName = Path.Combine(Util.GetDataFolderPath(), "users.config");
						var text = File.ReadAllText(fileName);

						_users = new Dictionary<string, string>();
						var lines = text.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
						foreach (var line in lines)
						{
							var parts = line.Split('\t');
							if (parts.Length == 2)
							{
								_users.Add(parts[0], parts[1]);
							}
						}
					}
				}
				catch (Exception exc)
				{
					Trace.WriteLine(exc);
				}
				var res = (_users == null) ? new Dictionary<string, string>() : new Dictionary<string, string>(_users);
				return res;
			}
		}
	}
}