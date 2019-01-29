using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	public class TaxReportsFactory : ITaxReportsFactory
	{
		public ReportRequestPropertiesContainer AmazonVATCalculationReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_SC_VAT_TAX_REPORT_", ContentUpdateFrequency.Unknown,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonEurope(),
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer AmazonVATTransactionsReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_VAT_TRANSACTION_DATA_", ContentUpdateFrequency.Unknown,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonEurope(),
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer OnDemandGSTMerchantTaxReportB2B(DateTime? startDate = null, DateTime? endDate = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_GST_MTR_B2B_CUSTOM_", ContentUpdateFrequency.Unknown,
				permittedMarketplaces: new MwsMarketplaceGroup(MwsMarketplace.India).GetMarketplacesIdList,
				requestedMarketplaces: null,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer OnDemandGSTMerchantTaxReportB2C(DateTime? startDate = null, DateTime? endDate = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_GST_MTR_B2C_CUSTOM_", ContentUpdateFrequency.Unknown,
				permittedMarketplaces: new MwsMarketplaceGroup(MwsMarketplace.India).GetMarketplacesIdList,
				requestedMarketplaces: null,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer SalesTaxReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException("This report cannot be requested or scheduled. You must generate the report from the Tax Document Library in Seller Central. After the report has been generated, you can download the report using the GetReportList and GetReport operations.");
		}
	}
}
