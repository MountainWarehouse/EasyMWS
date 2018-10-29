using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	public class OrderTrackingReportsFactory : IOrderTrackingReportsFactory
	{
		public ReportRequestPropertiesContainer FlatFileArchivedOrdersReport(DateTime? startDate = null, DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer FlatFileOrdersByLastUpdateReport(DateTime? startDate = null, DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer FlatFileOrdersByOrderDateReport(DateTime? startDate = null, DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer XMLOrdersByLastUpdateReport(DateTime? startDate = null, DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer XMLOrdersByOrderDateReport(DateTime? startDate = null, DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}
	}
}
