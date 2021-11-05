using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations.Contracts.Requests;
using MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations.Contracts.Responses;
using Newtonsoft.Json;

namespace MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations
{
    public class SimpleTradingHttpClient : ISimpleTradingHttpClient
    {
        private readonly string _baseUrl;
        public SimpleTradingHttpClient(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public async Task<Response<RegisterResponse, FailRegisterResponse>> 
            RegisterTraderAsync(RegisterRequest request)
        {
            var requestString = JsonConvert.SerializeObject(request);
            var result = await _baseUrl
                .AppendPathSegments("integration", "v1", "auth", "register")
                .WithHeader("Content-Type", "application/json")
                .AllowHttpStatus("400")
                .AllowHttpStatus("401")
                .PostJsonAsync(request);
            return await result.ResponseMessage.DeserializeTo<RegisterResponse, FailRegisterResponse>();
        }

        public async Task<Response<StatisticResponse, FailRegisterResponse>> 
            GetReportStatisticAsync(StatisticRequest request)
        {
            var result = await _baseUrl
                .AppendPathSegments("getStatus")
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(request);
            return await result.ResponseMessage.DeserializeTo<StatisticResponse, FailRegisterResponse>();
        }
    }
}