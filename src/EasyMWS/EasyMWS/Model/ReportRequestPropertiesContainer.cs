using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
		[IgnoreDataMember]
		public readonly string ReportType;

		#endregion

		#region Optional properties only used by some amazon MWS reports

		/// <summary>
		/// A list of one or more marketplace IDs for the marketplaces you are registered to sell in. <para />
		/// The resulting report will include information for all marketplaces you specify. <para />
		/// Example: &amp;MarketplaceIdList.Id.1=A13V1IB3VIYZZH &amp;MarketplaceIdList.Id.2=A1PA6795UKMFR9<para />
		/// </summary>
		public IEnumerable<string> MarketplaceIdList { get; set; }
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

		public bool IsSettlementReport { get; set; } = false;
		public string ReportId { get; set; } = null;

		/// <summary>
		/// Prevent clients from bypassing setting required properties as defined by the public constructor
		/// </summary>
		private ReportRequestPropertiesContainer()
		{
		}

		/// <summary>
		/// Initializes a wrapper containing part of the information needed to request a settlement report from Amazon.<br/>
		/// This constructor should be used to initialize reports of any type except for settlement reports.<br/>
		/// An alternative for getting an instance of this class is to use one of the factories available under namespace MountainWarehouse.EasyMWS.Factories.Reports<br/>
		/// </summary>
		/// <param name="reportType"></param>
		/// <param name="marketplaceIdList"></param>
		/// <param name="updateFrequency"></param>
		public ReportRequestPropertiesContainer(string reportType, ContentUpdateFrequency updateFrequency, IEnumerable<string> marketplaceIdList = null, DateTime? startDate = null, DateTime? endDate = null, string reportOptions = null)
		{
			ReportType = reportType;
			MarketplaceIdList = marketplaceIdList;
			UpdateFrequency = updateFrequency;
			StartDate = startDate;
			EndDate = endDate;
			ReportOptions = reportOptions;
		}

		/// <summary>
		/// Initializes a wrapper containing part of the information needed to request a settlement report from Amazon.<br/>
		/// This constructor should only be used to initialize settlement reports.<br/>
		/// An alternative for getting an instance of this class for a settlement report is to use the ISettlementReportsFactory<br/>
		/// </summary>
		/// <param name="settlementReportId"></param>
		public ReportRequestPropertiesContainer(string settlementReportId) 
			=> (ReportType, IsSettlementReport, ReportId) = ("_GET_V2_SETTLEMENT_REPORT_DATA_", true, settlementReportId);
	}
}