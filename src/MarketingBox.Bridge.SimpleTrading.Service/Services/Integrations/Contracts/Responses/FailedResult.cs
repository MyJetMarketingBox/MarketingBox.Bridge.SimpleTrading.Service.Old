using Newtonsoft.Json;

namespace MarketingBox.Bridge.SimpleTrading.Service.Services.Integrations.Contracts.Responses
{
    public class FailedResult
    {
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("statusCode")] public int StatusCode { get; set; }
    }
}