using System;
using System.Collections.Generic;
using MarketplaceWebService.Model;
using MountainWarehouse.EasyMWS.Enums;

namespace MountainWarehouse.EasyMWS.Helpers
{
	[Serializable]
	public class ReportRequestPropertiesContainer
	{
		#region Properties required for requesting all amazon MWS reports

		public string ReportType { get; set; }
		
		#endregion

		#region Optional properties only used by some amazon MWS reports

		public List<string> MarketplaceIdList { get; set; }

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
		public ReportRequestPropertiesContainer(string reportType, ContentUpdateFrequency updateFrequency, List<string> marketplaceIdList = null) =>
			(ReportType, MarketplaceIdList, UpdateFrequency) = (reportType, marketplaceIdList, updateFrequency);
	}
}

