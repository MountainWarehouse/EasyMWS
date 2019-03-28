using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	// When adding support for a new report type, ReportsPermittedMarketplacesMapper map also has to be updated to include the permitted marketplaces for that report.
	public class TaxReportsFactory : ITaxReportsFactory
	{
		public ReportRequestPropertiesContainer AmazonVATCalculationReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_SC_VAT_TAX_REPORT_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer AmazonVATTransactionsReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_VAT_TRANSACTION_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer OnDemandGSTMerchantTaxReportB2B(DateTime? startDate = null, DateTime? endDate = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_GST_MTR_B2B_CUSTOM_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: (List<string>)null,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer OnDemandGSTMerchantTaxReportB2C(DateTime? startDate = null, DateTime? endDate = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_GST_MTR_B2C_CUSTOM_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: (List<string>)null,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer SalesTaxReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException("This report cannot be requested or scheduled. You must generate the report from the Tax Document Library in Seller Central. After the report has been generated, you can download the report using the GetReportList and GetReport operations.");
		}
	}
}
