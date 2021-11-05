using Newtonsoft.Json;

namespace MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations.Contracts.Responses
{
    public class FailedResult
    {
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("statusCode")] public int StatusCode { get; set; }
    }
}