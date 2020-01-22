using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public class BrowseTreeReportFactory : IBrowseTreeReportFactory
    {
        public ReportRequestPropertiesContainer BrowseTreeReport(DateTime? startDate = null,
            DateTime? endDate = null,
            string marketplaceId = null,
            bool? rootNodesOnly = null,
            string browseNodeId = null)
        {
            var reportOptions = new ReportOptions();
            if (!string.IsNullOrWhiteSpace(marketplaceId))
            {
                reportOptions.AddStringOption("MarketplaceId", marketplaceId);
            }
            if (rootNodesOnly.HasValue)
            {
                reportOptions.AddBooleanOption("RootNodesOnly", rootNodesOnly.Value);
            }
            if (!string.IsNullOrWhiteSpace(browseNodeId))
            {
                reportOptions.AddStringOption("BrowseNodeId", browseNodeId);
            }
            if (!reportOptions.Options.Any()) reportOptions = null;

            var reportContainer = ReportGeneratorHelper.GenerateReportRequest("_GET_XML_BROWSE_TREE_DATA_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: (List<string>)null, startDate: startDate, endDate: endDate, reportOptions: reportOptions);

            return reportContainer;
        }
    }
}
