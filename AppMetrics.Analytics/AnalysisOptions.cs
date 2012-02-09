using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppMetrics.Analytics
{
	public class AnalysisOptions
	{
		public string ApplicationKey = "";

		public TimeSpan Period = TimeSpan.FromDays(1);

		public bool SliceByLocation = true;
		public bool SliceByFunction = true;

		public HashSet<string> CountryFilter = new HashSet<string>();
		public bool FilterByCountries { get { return CountryFilter.Count > 0; } }

		public override bool Equals(object obj)
		{
			var that = obj as AnalysisOptions;
			if (that == null)
				return false;

			var res = (ApplicationKey == that.ApplicationKey) && SliceByLocation == that.SliceByLocation &&
				SliceByFunction == that.SliceByFunction && CountryFilter.SequenceEqual(that.CountryFilter);
			return res;
		}

		public override int GetHashCode()
		{
			return ApplicationKey.GetHashCode() ^ CountryFilter.GetHashCode() ^ (SliceByLocation ^ SliceByFunction).GetHashCode();
		}

		public void Validate()
		{
			if (string.IsNullOrEmpty(ApplicationKey))
				throw new ArgumentException();
			if (!SliceByLocation && CountryFilter.Count > 0)
				throw new ArgumentException();
		}
	}
}
