using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;

namespace AppMetrics.WebUtils
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
			try
			{
				var application = (HttpApplication)sender;
				var context = application.Context;
				if (!Authenticate(context))
				{
					var response = context.Response;
					response.Status = "401 Unauthorized";
					response.StatusCode = 401;
					response.AddHeader("WWW-Authenticate", "Basic");
				}
			}
			catch (Exception exc)
			{
				WebLogger.Report(exc.ToString());
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
			var credentialsText = Encoding.ASCII.GetString(Convert.FromBase64String(base64Credentials));
			var credentials = credentialsText.Split(new[] { ':' });

			if (credentials.Length != 2 || string.IsNullOrEmpty(credentials[0]) || string.IsNullOrEmpty(credentials[1]))
				return null;

			return credentials;
		}

		private static IPrincipal TryGetPrincipal(string[] creds)
		{
			if (creds.Length != 2)
				return null;

			var userName = creds[0];
			var password = creds[1];

			var user = GetUser(userName);
			if (user != null)
			{
				if (CheckPassword(user, password))
					return new GenericPrincipal(new GenericIdentity(userName), new[] { "Administrator", "User" });
			}

			return null;
		}

		private static Dictionary<string, UserCredentials> _users;
		private static readonly object Sync = new object();

		static UserCredentials GetUser(string name)
		{
			lock (Sync)
			{
				try
				{
					if (_users == null)
					{
						var text = File.ReadAllText(CredentialsFileName);

						_users = new Dictionary<string, UserCredentials>();
						var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var line in lines)
						{
							var parts = line.Split('\t');
							if (parts.Length == 3)
							{
								var user = new UserCredentials
									{
										Name = parts[0],
										Salt = Encoding.Unicode.GetString(Convert.FromBase64String(parts[1])),
										PasswordHash = parts[2],
									};
								_users.Add(user.Name, user);
							}
						}
					}
				}
				catch (Exception exc)
				{
					WebLogger.Report(exc);
				}

				UserCredentials res;
				_users.TryGetValue(name, out res);
				return res;
			}
		}

		public static void CreateUser(string userName, string password)
		{
			lock (Sync)
			{
				_users = null;

				var random = new Random();
				var salt = new string((char)(random.Next(char.MaxValue)), 1);
				var hashString = GetPasswordHash(salt, password);
				var saltEncoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(salt));

				var newData = string.Format("{0}\t{1}\t{2}\r\n", userName, saltEncoded, hashString);
				File.AppendAllText(CredentialsFileName, newData);
			}
		}

		private static string GetPasswordHash(string salt, string password)
		{
			var saltedPassword = salt + password;
			var buf = Encoding.Unicode.GetBytes(saltedPassword);

			var algorithm = SHA256.Create();
			algorithm.TransformFinalBlock(buf, 0, buf.Length);
			var hash = algorithm.Hash;
			return Convert.ToBase64String(hash);
		}

		static bool CheckPassword(UserCredentials user, string password)
		{
			var hash = GetPasswordHash(user.Salt, password);
			var res = (user.PasswordHash == hash);
			return res;
		}

		public static string CredentialsFileName { get; set; }

		public static bool IsAnonymousAccessAllowed(HttpRequest request)
		{
			return UrlAuthorizationModule.CheckUrlAccessForPrincipal(request.Path, AnonymousUser, request.RequestType);
		}

		static readonly GenericPrincipal AnonymousUser = new GenericPrincipal(new GenericIdentity(""), new string[0]);

		class UserCredentials
		{
			public string Name;
			public string Salt;
			public string PasswordHash;
		}
	}
}