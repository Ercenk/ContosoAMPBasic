namespace Dashboard.Models
{
    using System.Net;

    using SaaSFulfillmentClient.Models;

    public class FulfillmentRequestErrorViewModel
    {
        public string RawResponse { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public static FulfillmentRequestErrorViewModel From(FulfillmentRequestResult result)
        {
            return new FulfillmentRequestErrorViewModel
                       {
                           RawResponse = result.RawResponse, StatusCode = result.StatusCode
                       };
        }
    }
}