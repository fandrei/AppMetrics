using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

using AppMetrics.Analytics;
using AppMetrics.Shared;
using AppMetrics.WebUtils;

namespace AppMetrics.AnalyticsSite
{
	/// <summary>
	/// Creates reports and delivers to the user
	/// </summary>
	public class GetReport : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			context.Server.ScriptTimeout = PageTimeout;
			context.Response.ContentType = "text/plain";

			var queryString = context.Request.Url.Query;
			try
			{
				Init();

				var requestTime = DateTime.UtcNow;
				var options = GetOptions(context.Request.QueryString);

				var watch = Stopwatch.StartNew();
				var results = CreateReport(options);
				watch.Stop();

				var callerIp = context.Request.UserHostAddress;
				ReportLog(string.Format("request from {0}: '{1}' generated in {2} secs",
						callerIp, queryString, watch.Elapsed.TotalSeconds));

				var status = string.Format("Period: {0}\tGenerated at: {1}\tGeneration time: {2} seconds\r\n",
					options.TimePeriod, requestTime.ToString("yyyy-MM-dd HH:mm:ss"), watch.Elapsed.TotalSeconds);
				context.Response.Write(status);

				string reportText;
				switch (options.ReportType)
				{
					case ReportType.LatencySummaries:
						reportText = Report.GetLatencyStatSummariesReport(results, options);
						break;
					case ReportType.LatencyDistribution:
						reportText = Report.GetLatencyDistributionReport(results);
						break;
					case ReportType.JitterDistribution:
						reportText = Report.GetJitterDistributionReport(results);
						break;
					case ReportType.StreamingLatencySummaries:
						reportText = Report.GetStreamingLatencyStatSummariesReport(results, options);
						break;
					case ReportType.StreamingLatencyDistribution:
						reportText = Report.GetStreamingLatencyDistributionReport(results);
						break;
					case ReportType.Exceptions:
						reportText = Report.GetExceptionsReport(results);
						break;
					default:
						throw new NotSupportedException();
				}

				context.Response.Write(reportText);
			}
			catch (ApplicationException exc)
			{
				context.Response.Write(exc.Message);
				var message = string.Format("request: '{0}' error '{1}'", queryString, exc.Message);
				ReportLog(message);
			}
			catch (Exception exc)
			{
				context.Response.Write(exc.ToString());
				var message = string.Format("request: '{0}' exception:\r\n{1}", queryString, exc);
				ReportLog(message);
			}
		}

		private const int PageTimeout = 5 * 60;

		private static AnalysisOptions GetOptions(NameValueCollection requestParams)
		{
			var application = requestParams.Get("Application");
			if (application == null)
				throw new ApplicationException("Application key is not defined");

			var res = new AnalysisOptions
			{
				ApplicationKey = application,
			};

			var sliceByLocationText = requestParams.Get("SliceByLocation");
			res.SliceByLocation = string.IsNullOrEmpty(sliceByLocationText)
				? LocationSliceType.None
				: (LocationSliceType)Enum.Parse(typeof(LocationSliceType), sliceByLocationText);

			var locations = requestParams.Get("Locations") ?? "";
			var locationList = locations.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			res.LocationIncludeOverall = (locationList.Length == 0);
			if (locationList.Contains("(World)"))
				res.LocationIncludeOverall = true;
			res.LocationFilter = new HashSet<string>(locationList);

			var functionFilter = requestParams.Get("FunctionFilter") ?? "";
			var functionFilterList = functionFilter.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			res.FunctionFilter = new HashSet<string>(functionFilterList);

			res.TimePeriod = TimePeriod.TryRead(requestParams);
			if (res.TimePeriod == null)
			{
				var periodString = requestParams.Get("Period") ?? "";
				var timeSpan = string.IsNullOrEmpty(periodString) ? DefaultReportPeriod : TimeSpan.Parse(periodString);
				res.TimePeriod = TimePeriod.Create(timeSpan);
			}

			var reportTypeText = requestParams.Get("Type");
			res.ReportType = string.IsNullOrEmpty(reportTypeText)
				? ReportType.LatencySummaries
				: (ReportType)Enum.Parse(typeof(ReportType), reportTypeText);

			if (res.ReportType == ReportType.Exceptions)
			{
				res.SliceByFunction = false;
			}
			else
			{
				var splitByFunctionsText = (requestParams.Get("SliceByFunctions") ?? "").ToLower();
				res.SliceByFunction = (splitByFunctionsText == "yes");
			}

			var sliceByNodeNameText = (requestParams.Get("SliceByNodeName") ?? "").ToLower();
			res.SliceByNodeName = (sliceByNodeNameText == "yes");

			return res;
		}

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}

		static List<CalcResult> CreateReport(AnalysisOptions options)
		{
			var sessions = LogReader.Parse(options);

			var convertor = new StatsBuilder();
			var res = convertor.Process(sessions, options);

			return res;
		}

		static string _logPath;

		static void Init()
		{
			if (_logPath == null)
				_logPath = Path.Combine(SiteConfig.AppDataPath, WebLogger.FileName);
		}

		static void ReportLog(string text)
		{
			try
			{
				var time = DateTime.UtcNow;
				var multiLineData = text.Contains('\n');
				var message = multiLineData
					? string.Format("{0}\r\n{1}\r\n{2}\r\n", time, text, Const.Delimiter)
					: string.Format("{0}\t{1}\r\n", time, text);
				File.AppendAllText(_logPath, message, Encoding.UTF8);
			}
			catch (Exception exc)
			{
				Trace.WriteLine(text);
				Trace.WriteLine(exc);
			}
		}

		private static readonly object Sync = new object();
		private static readonly TimeSpan DefaultReportPeriod = TimeSpan.FromMinutes(1);
		private static readonly TimeSpan CacheInvalidationPeriod = TimeSpan.FromSeconds(10);
	}
}