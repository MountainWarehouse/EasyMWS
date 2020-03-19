using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public class SettlementReportsFactory : ISettlementReportsFactory
    {
        public ReportRequestPropertiesContainer FlatFileSettlementReport(string reportId)
            => new ReportRequestPropertiesContainer(reportId);
    }
}
