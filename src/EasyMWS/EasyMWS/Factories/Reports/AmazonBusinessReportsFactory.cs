using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public class AmazonBusinessReportsFactory : IAmazonBusinessReportsFactory
    {
        public ReportRequestPropertiesContainer ManageQuotesReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
        => ReportGeneratorHelper.GenerateReportRequest("_RFQD_BULK_DOWNLOAD_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);
    }
}
