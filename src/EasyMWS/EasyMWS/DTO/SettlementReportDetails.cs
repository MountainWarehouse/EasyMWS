﻿using System;

namespace MountainWarehouse.EasyMWS.DTO
{
    /// <summary>
    /// SettlementReport details for a report which has already been generated by Amazon and is available for download.<br/>
    /// Mapped from : https://docs.developer.amazonservices.com/en_US/reports/Reports_Datatypes.html#ReportInfo
    /// </summary>
    public class SettlementReportDetails
    {
        /// <summary>
        /// The ReportType value requested.<br/>
        /// ReportType: https://docs.developer.amazonservices.com/en_US/reports/Reports_ReportType.html
        /// </summary>
        public readonly string ReportType;

        /// <summary>
        /// A unique Amazon report identifier.
        /// </summary>
        public readonly string ReportId;

        /// <summary>
        /// A Boolean value that indicates if an order report has been acknowledged by a prior call to UpdateReportAcknowledgements.<br/>
        /// Set to true to list order reports that have been acknowledged; set to false to list order reports that have not been acknowledged.<br/>
        /// This filter is valid only with order reports; it does not work with listing reports.<br/>
        /// </summary>
        public readonly bool IsAcknowledged;

        /// <summary>
        /// The date the report is available. In ISO 8601 date time format.<br/>
        /// https://docs.developer.amazonservices.com/en_US/dev_guide/DG_ISO8601.html
        /// </summary>
        public readonly DateTime AvailableDate;

        public SettlementReportDetails(string reportType, string reportId, bool isAcknowledged, DateTime availableDate)
        {
            ReportType = reportType;
            ReportId = reportId;
            IsAcknowledged = isAcknowledged;
            AvailableDate = availableDate;
        }
    }
}
