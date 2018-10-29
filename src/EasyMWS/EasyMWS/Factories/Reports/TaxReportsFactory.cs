using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	public class TaxReportsFactory : ITaxReportsFactory
	{
		public ReportRequestPropertiesContainer AmazonVATCalculationReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer AmazonVATTransactionsReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer SalesTaxReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}
	}
}
