using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;

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
			var context = application.Context;
			if (!Authenticate(context))
			{
				context.Response.Status = "401 Unauthorized";
				context.Response.StatusCode = 401;
				context.Response.AddHeader("WWW-Authenticate", "Basic");
				context.Response.End();
			}
		}

		static bool Authenticate(HttpContext context)
		{
			if (IsAnonymousAccessAllowed(context.Request))
				return true;

			var authHeader = context.Request.Headers.Get("Authorization");
			if (string.IsNullOrEmpty(authHeader))
				return false;

			var creds = ParseAuthHeader(authHeader);
			if (creds == null)
				return true; // anonymous user

			var principal = TryGetPrincipal(creds);
			
			context.User = principal;
			return true;
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

		private static IPrincipal TryGetPrincipal(string[] creds)
		{
			var users = GetUsers();

			foreach (var pair in users)
			{
				if (creds[0] == pair.Key && creds[1] == pair.Value)
				{
					return new GenericPrincipal(new GenericIdentity(pair.Key), new[] { "Administrator", "User" });
				}
			}

			return null;
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
						var fileName = Path.Combine(SiteConfig.DataStoragePath, "users.config");
						var text = File.ReadAllText(fileName);

						_users = new Dictionary<string, string>();
						var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
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

		public static bool IsAnonymousAccessAllowed(HttpRequest request)
		{
			return UrlAuthorizationModule.CheckUrlAccessForPrincipal(request.Path, AnonymousUser, request.RequestType);
		}

		static readonly GenericPrincipal AnonymousUser = new GenericPrincipal(new GenericIdentity(""), new string[0]);
	}
}