using System;
using System.Collections.Generic;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Helpers
{
	internal static class ReportGeneratorHelper
	{

		public static ReportRequestPropertiesContainer GenerateReportRequest(string reportType,
			ContentUpdateFrequency reportUpdateFrequency,
			List<string> permittedMarketplaces, List<string> requestedMarketplaces = null, DateTime? startDate = null,
			DateTime? endDate = null, ReportOptions reportOptions = null)
		{
			ValidateMarketplaceCompatibility(reportType, permittedMarketplaces, requestedMarketplaces);
			return new ReportRequestPropertiesContainer(reportType, reportUpdateFrequency, requestedMarketplaces, startDate,
				endDate, reportOptions?.GetOptionsString());
		}

		private static void ValidateMarketplaceCompatibility(string reportType, List<string> permittedMarketplaces,
			List<string> requestedMarketplaces = null)
		{
			if (requestedMarketplaces == null) return;

			foreach (var requestedMarketplace in requestedMarketplaces)
			{
				if (!permittedMarketplaces.Contains(requestedMarketplace))
				{
					throw new ArgumentException(
						$@"The report request for type:'{reportType}', is only available to the following marketplaces:'{
								MwsMarketplace.GetMarketplaceCountryCodesAsCommaSeparatedString(permittedMarketplaces)
							}'.
The requested marketplace:'{
								MwsMarketplace.GetMarketplaceCountryCode(requestedMarketplace)
							}' is not supported by Amazon MWS for the specified report type.");
				}
			}
		}
	}
}