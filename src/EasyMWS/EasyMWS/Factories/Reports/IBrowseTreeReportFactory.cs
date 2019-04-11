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
        /// Can be requested or scheduled. For Marketplace and Seller Central sellers.<para/>
        /// To keep track of which browse nodes change over time, Amazon recommends that each time you request this report you compare it to the last report you requested using the same ReportOptions values.<para/>
        /// URL-encoded example: ReportOptions=MarketplaceId%3DATVPDKIKX0DER;BrowseNodeId%3D15706661<para/>
        /// The Browse Tree Report is described by the following XSD: BrowseTreeReport.xsd.<para/>
        /// Note: As Amazon updates the Amazon MWS Reports API section, Amazon may update the BrowseTreeReport.xsd schema. Keep this in mind if you choose to use this schema for validation.<para/>
        /// Monitor an Amazon MWS discussion forum for announcements of updates to the BrowseTreeReport.xsd schema. You can find the Amazon MWS discussion forums here:<para/>
        /// Chinese: https://mai.amazon.cn/forums/forum.jspa?forumID=15 <para/>
        /// English: https://sellercentral.amazon.com/forums/forum.jspa?forumID=35 <para/>
        /// Japanese: https://sellercentral.amazon.co.jp/forums/forum.jspa?forumID=14 <para/>
        /// </summary>
        /// <returns></returns>
        ReportRequestPropertiesContainer FlatFileOrdersByLastUpdateReport();
    }
}
