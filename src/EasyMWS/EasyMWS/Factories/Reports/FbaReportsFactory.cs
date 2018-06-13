using System;
using System.Collections.Generic;
using System.Linq;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	public class FbaReportsFactory : IFbaReportsFactory
	{
		#region FBA Inventory Reports

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaAmazonFulfilledInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_AFN_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaMultiCountryInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_AFN_INVENTORY_DATA_BY_COUNTRY_",
				ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonEurope(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaManageExcessInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_EXCESS_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaCrossBorderInventoryMovementReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CROSS_BORDER_INVENTORY_MOVEMENT_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaDailyInventoryHistoryReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CURRENT_INVENTORY_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaInboundPerformanceReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INBOUND_NONCOMPLIANCE_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaInventoryAdjustmentsReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_ADJUSTMENTS_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaInventoryHealthReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_HEALTH_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaReceivedInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_RECEIPTS_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaInventoryEventDetailReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_SUMMARY_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaMonthlyInventoryHistoryReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_MONTHLY_INVENTORY_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaInventoryAgeReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_INVENTORY_AGED_DATA_", ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaManageInventoryArchived(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_MYI_ALL_INVENTORY_DATA_",
				ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaManageInventory(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_MYI_UNSUPPRESSED_INVENTORY_DATA_",
				ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaReservedInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_RESERVED_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer RestockInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_RESTOCK_INVENTORY_RECOMMENDATIONS_REPORT_",
				ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: new List<string> {MwsMarketplace.US.Id},
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaBulkFixStrandedInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_STRANDED_INVENTORY_LOADER_DATA_",
				ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaStrandedInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_STRANDED_INVENTORY_UI_DATA_",
				ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		#endregion

		#region FBA Payment Reports

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaFeePreviewReport(DateTime startDate,
			DateTime? endDate, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_ESTIMATED_FBA_FEES_TXT_DATA_",
				ContentUpdateFrequency.AtLeast72Hours,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonEurope() + MwsMarketplace.US + MwsMarketplace.Canada +
				                       MwsMarketplace.Mexico,
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate ?? DateTime.UtcNow);

		#endregion

		#region FBA Customer Concessions Reports

		public ReportRequestPropertiesContainer FbaReturnsReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CUSTOMER_RETURNS_DATA_",
				ContentUpdateFrequency.Daily,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		#endregion
	}
}
