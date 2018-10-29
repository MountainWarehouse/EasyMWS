using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	public class PendingOrderReportsFactory : IPendingOrderReportsFactory
	{
		public ReportRequestPropertiesContainer ConvergedFlatFilePendingOrdersReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer FlatFilePendingOrdersReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer XMLPendingOrdersReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}
	}
}
