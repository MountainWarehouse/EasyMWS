using MountainWarehouse.EasyMWS.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public interface IEasyShipReportsFactory
    {
        /// <summary>
        /// Generate a request object for a MWS report of type : _GET_EASYSHIP_DOCUMENTS_ <para />
        /// PDF report that contains the invoice, shipping label, and warranty (if available) documents for the Amazon Easy Ship order. This report is only available in the India marketplace.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        ReportRequestPropertiesContainer EasyShipReport(DateTime? startDate = null, DateTime? endDate = null);
    }
}
