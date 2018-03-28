using System.Net;

namespace MountainWarehouse.EasyMWS.Logging
{
    public class RequestInfo
    {
	    public readonly string RequestId;
	    public readonly string Timestamp;
	    public readonly HttpStatusCode? StatusCode;
	    public readonly string ErrorType;
	    public readonly string ErrorCode;

		private RequestInfo()
	    {
	    }

	    internal RequestInfo(string timestamp, string requestId,  HttpStatusCode? statusCode = null, string errorType = null, string errorCode = null)
			=> (Timestamp, RequestId, StatusCode, ErrorType, ErrorCode) 
			= (timestamp, requestId, statusCode, errorType, errorCode);
    }
}
