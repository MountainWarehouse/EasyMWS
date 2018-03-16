using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Helpers
{
	[Serializable]
    public class FeedSubmissionPropertiesContainer
    {
		#region Properties required for submitting any feed type to amazon

		/// <summary>
		/// The actual content of the feed itself, in XML or flat file format.
		/// </summary>
		public string FeedContent { get; set; }

		/// <summary>
		/// A FeedType value indicating how the data should be processed.
		/// </summary>
		public string FeedType { get; set; }

		#endregion

		#region Optional properties only used for submitting some feed types tp amazon

		/// <summary>
		/// A Boolean value that enables the purge and replace functionality. Set to true to purge and replace the existing data; otherwise false. <para />
		/// This value only applies to product-related flat file feed types, which do not have a mechanism for specifying purge and replace in the feed body. <para />
		/// Use this parameter only in exceptional cases. Usage is throttled to allow only one purge and replace within a 24-hour period.<para />
		/// </summary>
		public bool? PurgeAndReplace { get; set; }

		/// <summary>
		/// An MD5 hash of the feed content. Amazon MWS uses this value to determine if 
		/// the feed data has been corrupted or tampered with during transit. This replaces the Content-MD5 header.
		/// </summary>
		public string ContentMD5Value { get; set; }

		#endregion

		/// <summary>
		/// A list of one or more marketplace IDs (of marketplaces you are registered to sell in) that you want the feed to be applied to.  <para />
		/// The feed will be applied to all the marketplaces you specify. For example: <para />
		/// &amp;MarketplaceIdList.Id.1=A13V1IB3VIYZZH &amp;MarketplaceIdList.Id.2=A1PA6795UKMFR9 <para />
		/// </summary>
		public List<string> MarketplaceIdList { get; set; }

	    private FeedSubmissionPropertiesContainer()
	    {
	    }

		/// <summary>
		/// Creates a new feed submission wrapper object.
		/// </summary>
		/// <param name="feedContent"></param>
		/// <param name="feedType"></param>
		/// <param name="purgeAndReplace"></param>
		/// <param name="mdContentMd5Value"></param>
		/// <param name="marketplaceIdList"></param>
		public FeedSubmissionPropertiesContainer(string feedContent, string feedType, bool? purgeAndReplace = null,
		    string mdContentMd5Value = null, List<string> marketplaceIdList = null) =>
		    (FeedContent, FeedType, PurgeAndReplace, ContentMD5Value, MarketplaceIdList) =
		    (feedContent, feedType, purgeAndReplace, ContentMD5Value, marketplaceIdList);
    }
}
