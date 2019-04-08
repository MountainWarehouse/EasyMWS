using MountainWarehouse.EasyMWS.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public interface IAmazonBusinessReportsFactory
    {
        /// <summary>
        /// Generate a request object for a MWS report of type : _RFQD_BULK_DOWNLOAD_ <para />
        /// A Microsoft Excel Workbook (.xlsx) file. Contains current details of requests for quantity discounts including customer requests, active quantity discounts, analysis of pending requests, and analysis of all requests. <para/>
        /// Content updated in near real time. For Amazon Business sellers only. This report is only available in the US, UK, Germany, India, and Japan marketplaces.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="requestedMarketplaces"></param>
        /// <returns></returns>
        ReportRequestPropertiesContainer ManageQuotesReport(DateTime? startDate = null, DateTime? endDate = null,
            IEnumerable<MwsMarketplace> requestedMarketplaces = null);
    }
}
