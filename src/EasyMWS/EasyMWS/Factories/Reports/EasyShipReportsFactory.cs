using System;
using System.Collections.Generic;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public class EasyShipReportsFactory : IEasyShipReportsFactory
    {
        public ReportRequestPropertiesContainer EasyShipReport(DateTime? startDate = null, DateTime? endDate = null)
        => ReportGeneratorHelper.GenerateReportRequest("_GET_EASYSHIP_DOCUMENTS_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: (List<string>)null, startDate: startDate, endDate: endDate);
    }
}
