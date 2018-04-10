using System;
using System.Collections.Generic;
using MountainWarehouse.EasyMWS.Enums;

namespace MountainWarehouse.EasyMWS.Model
{
	[Serializable]
	public class ReportRequestPropertiesContainer
	{
		#region Properties required for requesting all amazon MWS reports

		/// <summary>
		/// A value of the ReportType that indicates the type of report to request.
		/// </summary>
		public string ReportType { get; set; }

		#endregion

		#region Optional properties only used by some amazon MWS reports

		/// <summary>
		/// A list of one or more marketplace IDs for the marketplaces you are registered to sell in. <para />
		/// The resulting report will include information for all marketplaces you specify. <para />
		/// Example: &amp;MarketplaceIdList.Id.1=A13V1IB3VIYZZH &amp;MarketplaceIdList.Id.2=A1PA6795UKMFR9<para />
		/// </summary>
		public List<string> MarketplaceIdList { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string ReportOptions { get; set; }

		#endregion


		/// <summary>
		/// The frequency with which amazon updates information for the Report request type attached to this report request wrapper.<para />
		/// This information can be used to reduce the number of report request made, especially if the requests are redundant.<para/>
		/// If the content for a report type is only updated Daily by amazon, then it shouldn't be requested on a base more frequent than daily. 
		/// </summary>
		public ContentUpdateFrequency UpdateFrequency;

		/// <summary>
		/// Prevent clients from bypassing setting required properties as defined by the public constructor
		/// </summary>
		private ReportRequestPropertiesContainer()
		{
		}

		/// <summary>
		/// Creates a new RequestReportRequest wrapper object that also contains additional information regarding the request object.
		/// </summary>
		/// <param name="reportType"></param>
		/// <param name="marketplaceIdList"></param>
		/// <param name="updateFrequency"></param>
		public ReportRequestPropertiesContainer(string reportType, ContentUpdateFrequency updateFrequency, List<string> marketplaceIdList = null, DateTime? startDate = null, DateTime? endDate = null, string reportOptions = null)
		{
			if (string.IsNullOrEmpty(reportType))
				throw new ArgumentException("ReportType was not specified, but it is required.");

			ReportType = reportType;
			MarketplaceIdList = marketplaceIdList;
			UpdateFrequency = updateFrequency;
			StartDate = startDate;
			EndDate = endDate;
			ReportOptions = reportOptions;
		}
	}
}

