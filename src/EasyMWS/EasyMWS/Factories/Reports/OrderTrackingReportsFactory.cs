using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	// When adding support for a new report type, ReportsPermittedMarketplacesMapper map also has to be updated to include the permitted marketplaces for that report.
	public class OrderTrackingReportsFactory : IOrderTrackingReportsFactory
	{
		public ReportRequestPropertiesContainer FlatFileArchivedOrdersReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_ARCHIVED_ORDERS_DATA_BY_ORDER_DATE_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FlatFileOrdersByLastUpdateReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_ALL_ORDERS_DATA_BY_LAST_UPDATE_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FlatFileOrdersByOrderDateReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_ALL_ORDERS_DATA_BY_ORDER_DATE_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer XMLOrdersByLastUpdateReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
        => ReportGeneratorHelper.GenerateReportRequest("_GET_XML_ALL_ORDERS_DATA_BY_LAST_UPDATE_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);


		public ReportRequestPropertiesContainer XMLOrdersByOrderDateReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
        => ReportGeneratorHelper.GenerateReportRequest("_GET_XML_ALL_ORDERS_DATA_BY_ORDER_DATE_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);
	}
}
