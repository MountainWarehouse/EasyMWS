using System;
using System.Collections.Generic;
using System.Text;
using MarketplaceWebService;

namespace EasyMWS
{
    public class EasyMwsClient
    {

	    public EasyMwsClient()
	    {
		    var client = new MarketplaceWebServiceClient("", "", "", "");
	    }

    }
}
