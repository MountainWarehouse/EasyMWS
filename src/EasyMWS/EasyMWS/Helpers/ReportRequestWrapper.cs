using System;
using System.Collections.Generic;
using System.Text;
using MarketplaceWebService.Model;

namespace MountainWarehouse.EasyMWS.Helpers
{
	/// <summary>
	/// A wrapper that attaches additional useful data or information, to an object of type RequestReportRequest.
	/// </summary>
	public class ReportRequestWrapper
	{
		/// <summary>
		/// The actual RequestReportRequest object that can be passed to the MWS client to request a report.
		/// </summary>
		public readonly RequestReportRequest ReportRequest;

		/// <summary>
		/// The frequency with which amazon updates information for the Report request type attached to this report request wrapper.<para />
		/// This information can be used to reduce the number of report request made, especially if the requests are redundant.<para/>
		/// If the content for a report type is only updated Daily by amazon, then it shouldn't be requested on a base more frequent than daily. 
		/// </summary>
		public readonly ContentUpdateFrequency UpdateFrequency;

		/// <summary>
		/// Creates a new RequestReportRequest wrapper object that also contains additional information regarding the request object.
		/// </summary>
		/// <param name="reportRequest"></param>
		/// <param name="updateFrequency"></param>
		public ReportRequestWrapper(RequestReportRequest reportRequest, ContentUpdateFrequency updateFrequency) =>
			(ReportRequest, UpdateFrequency) = (reportRequest, updateFrequency);
	}

	/// <summary>
	/// Describes how often content is updated for an amazon MWS web service endpoint.
	/// </summary>
	public enum ContentUpdateFrequency
	{
		NearRealTime,
		Daily,
		Unknown
	}
}
