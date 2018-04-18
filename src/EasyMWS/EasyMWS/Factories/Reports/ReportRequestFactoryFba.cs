using System;
using System.Collections.Generic;
using System.Linq;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	public class ReportRequestFactoryFba : IReportRequestFactoryFba
	{
		#region FBA Inventory Reports

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetAfnInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_AFN_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetAfnInventoryDataByCountry(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_AFN_INVENTORY_DATA_BY_COUNTRY_",
				ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonEurope(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetExcessInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_EXCESS_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CROSS_BORDER_INVENTORY_MOVEMENT_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CURRENT_INVENTORY_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INBOUND_NONCOMPLIANCE_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_ADJUSTMENTS_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInventoryHealthData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_HEALTH_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_RECEIPTS_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInventorySummaryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_SUMMARY_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_MONTHLY_INVENTORY_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaInventoryAgedData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_INVENTORY_AGED_DATA_", ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaMyiAllInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_MYI_ALL_INVENTORY_DATA_",
				ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_MYI_UNSUPPRESSED_INVENTORY_DATA_",
				ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetReservedInventoryData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_RESERVED_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		public ReportRequestPropertiesContainer GenerateRestockInventoryReport(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_RESTOCK_INVENTORY_RECOMMENDATIONS_REPORT_",
				ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: new List<string>{MwsMarketplace.US.Id},
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetStrandedInventoryLoaderData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_STRANDED_INVENTORY_LOADER_DATA_",
				ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportGetStrandedInventoryUiData(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_STRANDED_INVENTORY_UI_DATA_",
				ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		#endregion

		#region FBA Payment Reports

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer GenerateRequestForReportFbaFeePreviewReport(DateTime startDate,
			DateTime? endDate,
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_ESTIMATED_FBA_FEES_TXT_DATA_",
				ContentUpdateFrequency.AtLeast72Hours,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonEurope() + MwsMarketplace.US + MwsMarketplace.Canada +
				                       MwsMarketplace.Mexico,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate ?? DateTime.UtcNow);

		#endregion

		#region FBA Customer Concessions Reports

		public ReportRequestPropertiesContainer FbaReturnsReport(
			MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CUSTOMER_RETURNS_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

		#endregion
	}
}
