using System;
using System.Collections.Generic;
using System.Linq;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Helpers
{
	public static class ReportGeneratorHelper
	{
		public static ReportRequestPropertiesContainer GenerateReportRequest(string reportType,
			ContentUpdateFrequency reportUpdateFrequency, IEnumerable<string> requestedMarketplaces = null, DateTime? startDate = null,
			DateTime? endDate = null, ReportOptions reportOptions = null)
		{
			ValidateMarketplaceCompatibility(reportType, requestedMarketplaces);
			return new ReportRequestPropertiesContainer(reportType, reportUpdateFrequency, requestedMarketplaces, startDate,
				endDate, reportOptions?.GetOptionsString());
		}

		public static ReportRequestPropertiesContainer GenerateReportRequest(string reportType, ContentUpdateFrequency reportUpdateFrequency,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null, 
			DateTime? startDate = null, DateTime? endDate = null, ReportOptions reportOptions = null)
			=> GenerateReportRequest(reportType, reportUpdateFrequency, requestedMarketplaces?.Select(m => m.Id), 
				startDate, endDate, reportOptions);

		private static void ValidateMarketplaceCompatibility(string reportType, IEnumerable<string> requestedMarketplacesIds = null)
		{
			if (requestedMarketplacesIds == null) return;

			var permittedMarketplacesIds = ReportsPermittedMarketplacesMapper.GetMarketplaces(reportType)?.Select(m => m.Id) ?? MwsMarketplaceGroup.AmazonGlobal().Select(m => m.Id);

			foreach (var requestedMarketplace in requestedMarketplacesIds)
			{
				if (!permittedMarketplacesIds.Contains(requestedMarketplace))
				{
					throw new ArgumentException(
						$@"The report request for type:'{reportType}', is only available to the following marketplaces:'{
								MwsMarketplace.GetMarketplaceCountryCodesAsCommaSeparatedString(permittedMarketplacesIds)
							}'.
The requested marketplace:'{
								MwsMarketplace.GetMarketplaceCountryCode(requestedMarketplace)
							}' is not supported by Amazon MWS for the specified report type");
				}
			}
		}
	}
}