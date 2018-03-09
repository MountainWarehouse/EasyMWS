using System;
using System.Collections.Generic;
using System.Linq;
using MarketplaceWebService.Model;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	public class ReportRequestFactoryFba : IReportRequestFactoryFba
	{
		private readonly string _merchant;
		private readonly string _mWsAuthToken;

		/// <summary>
		/// Creates an instance of the Report Request Factory for amazon FBA reports.
		/// </summary>
		/// <param name="merchant">Optional parameter. MerchantId / SellerId</param>
		/// <param name="mWsAuthToken">MWS request authentication token</param>
		public ReportRequestFactoryFba(string merchant = null, string mWsAuthToken = null)
			=> (_merchant, _mWsAuthToken)
				= (merchant, mWsAuthToken);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetAfnInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_AFN_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetAfnInventoryDataByCountry(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_AFN_INVENTORY_DATA_BY_COUNTRY_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonEurope(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetExcessInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_EXCESS_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_FBA_FULFILLMENT_CROSS_BORDER_INVENTORY_MOVEMENT_DATA_", ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_FBA_FULFILLMENT_CURRENT_INVENTORY_DATA_", ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_FBA_FULFILLMENT_INBOUND_NONCOMPLIANCE_DATA_", ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_ADJUSTMENTS_DATA_", ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInventoryHealthData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_HEALTH_DATA_", ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_RECEIPTS_DATA_", ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInventorySummaryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_SUMMARY_DATA_", ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_FBA_FULFILLMENT_MONTHLY_INVENTORY_DATA_", ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaInventoryAgedData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_FBA_INVENTORY_AGED_DATA_", ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaMyiAllInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_FBA_MYI_ALL_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_FBA_MYI_UNSUPPRESSED_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetReservedInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_RESERVED_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetRestockInventoryRecommendationsReport(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_RESTOCK_INVENTORY_RECOMMENDATIONS_REPORT_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetStrandedInventoryLoaderData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_STRANDED_INVENTORY_LOADER_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetStrandedInventoryUiData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> GenerateReportRequest("_GET_STRANDED_INVENTORY_UI_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		private ReportRequestPropertiesContainer GenerateReportRequest(string reportType, ContentUpdateFrequency reportUpdateFrequency,
			List<string> permittedMarketplaces, List<string> requestedMarketplaces = null)
		{
			ValidateMarketplaceCompatibility(reportType, permittedMarketplaces, requestedMarketplaces);
			return new ReportRequestPropertiesContainer(reportType, _merchant, _mWsAuthToken, reportUpdateFrequency,
				requestedMarketplaces);
		}

		private void ValidateMarketplaceCompatibility(string reportType, List<string> permittedMarketplaces,
			List<string> requestedMarketplaces = null)
		{
			if (requestedMarketplaces == null) return;

			foreach (var requestedMarketplace in requestedMarketplaces)
			{
				if (!permittedMarketplaces.Contains(requestedMarketplace))
				{
					throw new ArgumentException(
						$@"The report request for type:'{reportType}', is only available to the following marketplaces:'{
								permittedMarketplaces.Aggregate((c, n) => $"{c}, {n}")
							}'.
The requested marketplace:'{requestedMarketplace}' is not supported by Amazon MWS for the specified report type.");
				}
			}
		}
	}
}
