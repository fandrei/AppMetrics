using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.ServiceModel.Web;
using AppMetrics.DataModel;

namespace AppMetrics
{
	public class DataService : DataService<DataSource>
	{
		// This method is called only once to initialize service-wide policies.
		public static void InitializeService(DataServiceConfiguration config)
		{
			config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
			config.UseVerboseErrors = true;

			config.SetEntitySetAccessRule("Sessions", EntitySetRights.All);
			config.SetEntitySetAccessRule("Records", EntitySetRights.All);

			config.SetServiceOperationAccessRule("GetSessions", ServiceOperationRights.AllRead);
		}

		protected override void HandleException(HandleExceptionArgs args)
		{
			base.HandleException(args);
			Trace.WriteLine(args.Exception);
		}

		[WebGet]
		public Session[] GetSessions(string appKey, string period)
		{
			var periodSpan = TimeSpan.Parse(period);
			return DataSource.GetSessions(appKey, periodSpan).ToArray();
		}
	}
}
