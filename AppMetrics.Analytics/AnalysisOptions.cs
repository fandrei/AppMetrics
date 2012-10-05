using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AppMetrics.Shared;

namespace AppMetrics.Analytics
{
	public enum LocationSliceType { None, Countries, CountriesAndCities }
	public enum ReportType { LatencySummaries, LatencyDistribution, JitterDistribution,
		StreamingLatencySummaries, StreamingLatencyDistribution, Exceptions }

	public class AnalysisOptions
	{
		public string ApplicationKey = "";

		public TimePeriod TimePeriod;

		public bool SliceByFunction = true;
		public bool SliceByNodeName = false;

		public LocationSliceType SliceByLocation = LocationSliceType.CountriesAndCities;
		public bool LocationIncludeOverall;

		public HashSet<string> LocationFilter = new HashSet<string>();
		public bool FilterByLocation { get { return LocationFilter.Count > 0; } }

		public bool LocationIsAllowed(Location loc)
		{
			if (!FilterByLocation)
				return true;

			if (loc == null)
				return false;

			var locName = Util.GetLocationName(loc);
			var res = LocationFilter.Any(locName.StartsWith);
			return res;
		}

		public HashSet<string> FunctionFilter = new HashSet<string>();
		public bool FilterByFunction { get { return FunctionFilter.Count > 0; } }

		public bool FunctionIsAllowed(string functionName)
		{
			if (!FilterByFunction)
				return true;

			var res = FunctionFilter.Any(functionName.StartsWith);
			return res;
		}

		public ReportType ReportType;

		public void Validate()
		{
			if (SliceByLocation == LocationSliceType.None && LocationFilter.Count > 0)
				throw new ArgumentException();
		}
	}
}
