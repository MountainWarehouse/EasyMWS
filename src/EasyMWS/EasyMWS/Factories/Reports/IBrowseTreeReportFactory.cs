using MountainWarehouse.EasyMWS.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public interface IBrowseTreeReportFactory
    {
        /// <summary>
        /// Generate a request object for a MWS report of type : _GET_XML_BROWSE_TREE_DATA_ <para />
        /// XML report that provides browse tree hierarchy information and node refinement information for the Amazon retail website in any marketplace.<para/>
        /// Note: If RootNodesOnly and BrowseNodeId are both included in the ReportOptions parameter, RootNodesOnly takes precedence.<para/>
        /// Note: Amazon recommends that you do not include the MarketplaceIdList parameter with calls to the RequestReport operation that request the Browse Tree Report. If there is ever a conflict between a MarketplaceIdList parameter value and the MarketplaceId value of the ReportOptions parameter, the MarketplaceId value takes precedence.<para/>
        /// To keep track of which browse nodes change over time, Amazon recommends that each time you request this report you compare it to the last report you requested using the same ReportOptions values.<para/>
        /// The Browse Tree Report is described by the following XSD: https://images-na.ssl-images-amazon.com/images/G/01/mwsportal/doc/en_US/Reports/XSDs/BrowseTreeReport.xsd<para/>
        /// Note: As Amazon updates the Amazon MWS Reports API section, we may update the BrowseTreeReport.xsd schema. Keep this in mind if you choose to use this schema for validation. Monitor an Amazon MWS discussion forum for announcements of updates to the BrowseTreeReport.xsd schema. You can find the Amazon MWS discussion forums here: English: https://sellercentral.amazon.com/forums/forum.jspa?forumID=35<para/>
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="marketplaceId">Specifies the marketplace from which you want browse tree information. Optional. If MarketplaceId is not included in the ReportOptions parameter, the report contains browse tree information from your default marketplace.<para/>Note: You must be registered as a seller in any marketplace that you specify using the MarketplaceId value. Also, your request must be sent to an endpoint that corresponds to the MarketplaceId that you specify. Otherwise the service returns an error.<para/>You can find a list of MarketplaceId values and endpoints in the "Amazon MWS endpoints and MarketplaceId values" section of the Amazon MWS Developer Guide.</param>
        /// <param name="rootNodesOnly">Type: xs:boolean. Optional. If true, then the report contains only the root nodes from the marketplace specified using MarketplaceId (or from your default marketplace, if MarketplaceId is not specified).<para/>If false, or if RootNodesOnly is not included in the ReportOptions parameter, then the content of the report depends on the value of BrowseNodeId.</param>
        /// <param name="browseNodeId">Specifies the top node of the browse tree hierarchy in the report. Optional. If BrowseNodeId is not included in the ReportOptions parameter, <para/>and if RootNodesOnly is false or is not included in the ReportOptions parameter, then the report contains the entire browse node hierarchy from the marketplace specified using MarketplaceId (or from your default marketplace, if MarketplaceId is not specified).<para/>Note that if you include an invalid BrowseNodeId in your request, the service returns a report that contains no data.</param>
        /// <returns></returns>
        ReportRequestPropertiesContainer BrowseTreeReport(
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            string marketplaceId = null,
            bool? rootNodesOnly = null,
            string browseNodeId = null);
    }
}
