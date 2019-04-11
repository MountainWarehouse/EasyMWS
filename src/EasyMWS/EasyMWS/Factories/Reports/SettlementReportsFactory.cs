using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	// When adding support for a new report type, ReportsPermittedMarketplacesMapper map also has to be updated to include the permitted marketplaces for that report.
	public class SettlementReportsFactory : ISettlementReportsFactory
	{
		public ReportRequestPropertiesContainer FlatFileSettlementReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException("Tab-delimited flat file settlement report that is automatically scheduled by Amazon; it cannot be requested through RequestReport");
		}

		public ReportRequestPropertiesContainer FlatFileV2SettlementReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException("XML file settlement report that is automatically scheduled by Amazon; it cannot be requested through RequestReport");
		}

		public ReportRequestPropertiesContainer XmlSettlementReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException("Tab-delimited flat file alternate version of the Flat File Settlement Report that is automatically scheduled by Amazon; it cannot be requested through RequestReport");
		}
	}
}
