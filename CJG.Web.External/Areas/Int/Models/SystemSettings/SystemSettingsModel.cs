using System.Collections.Generic;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.SystemSettings
{
	public class SystemSettingsModel : BaseViewModel
	{
		public bool EiEligibilityCheckServiceState { get; set; }
		public List<KeyValuePair<string, bool>> StateOptions { get; set; }

		public SystemSettingsModel()
		{
			var options = new List<KeyValuePair<string, bool>>
			{
				new KeyValuePair<string, bool>("On", true),
				new KeyValuePair<string, bool>("Off", false)
			};

			StateOptions = options;
		}
	}
}