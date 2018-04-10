using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Services
{
	/// <summary>
	/// A service that can provide access to Amazon reports stored in the EasyMws internal database.<para/>
	/// Please note that Amazon reports are Not stored in the internal database by default.
	/// This behavior can be overridden by setting the following EasyMws option to true : KeepAmazonReportsInLocalDbAfterCallbackIsPerformed.
	/// Please note that any stored report is deleted automatically after a period of time configured with the following EasyMws option : KeepAmazonReportsLocallyForTimePeriod.
	/// </summary>
	public interface IInternalStorageReportService
	{
		/// <summary>
		/// Retrieves the complete list of reports downloaded from Amazon and stored in the EasyMws internal database.<para/>
		/// </summary>
		/// <returns></returns>
		IEnumerable<AmazonReport> GetAllReports();

		/// <summary>
		/// Get a list of reports filtered with the specified lambda expression, from the EasyMws internal database.<para/>
		/// </summary>
		/// <returns></returns>
		IQueryable<AmazonReport> GetReportsWhere(Expression<Func<AmazonReport, bool>> predicate);

		/// <summary>
		/// Retrieves the complete list of reports with the specified report type, downloaded from Amazon and stored in the EasyMws internal database.<para/>
		/// The complete list of report types is available in the AmazonServices documentation found at https://docs.developer.amazonservices.com/en_US/reports/Reports_ReportType.html
		/// </summary>
		/// <param name="reportType"></param>
		/// <returns></returns>
		IEnumerable<AmazonReport> GetAllReportsWithType(string reportType);

		/// <summary>
		/// Deletes all reports downloaded from Amazon and stored in the EasyMws internal database.<para/>
		/// </summary>
		void DeleteAllReportsFromInternalStorage();
	}
}
