using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	/// <summary>
	/// When adding support for a new report type, ReportsPermittedMarketplacesMapper map also has to be updated to include the permitted marketplaces for that report.
	/// </summary>
	public class SettlementReportsFactory : ISettlementReportsFactory
	{
		public ReportRequestPropertiesContainer FlatFileSettlementReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer FlatFileV2SettlementReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer XmlSettlementReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}
	}
}
