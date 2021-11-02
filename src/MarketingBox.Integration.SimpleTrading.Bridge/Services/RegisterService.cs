using System;
using System.Threading.Tasks;
using MarketingBox.Integration.Bridge.Client;
using MarketingBox.Integration.Service.Grpc.Models.Common;
using MarketingBox.Integration.Service.Grpc.Models.Leads;
using MarketingBox.Integration.Service.Grpc.Models.Leads.Contracts;
using MarketingBox.Integration.Service.Grpc.Models.Reporting;
using MarketingBox.Integration.SimpleTrading.Bridge.Domain.Extensions;
using MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations;
using MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations.Contracts.Enums;
using MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations.Contracts.Requests;
using MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations.Contracts.Responses;
using Microsoft.Extensions.Logging;

namespace MarketingBox.Integration.SimpleTrading.Bridge.Services
{
    public class RegisterService : IBridgeService
    {
        private readonly ILogger<RegisterService> _logger;
        private readonly ISimpleTradingHttpClient _simpleTradingHttpClient;

        public RegisterService(ILogger<RegisterService> logger,
            ISimpleTradingHttpClient simpleTradingHttpClient)
        {
            _logger = logger;
            _simpleTradingHttpClient = simpleTradingHttpClient;
        }

        public async Task<RegistrationBridgeResponse> RegisterCustomerAsync(
            RegistrationBridgeRequest request)
        {
            _logger.LogInformation("Creating new LeadInfo {@context}", request);
            try
            {
                var registerResult =
                    await _simpleTradingHttpClient.RegisterTraderAsync(new RegisterRequest()
                    {
                        FirstName = request.Info.FirstName,
                        LastName = request.Info.LastName,
                        Password = request.Info.Password,
                        Email = request.Info.Email,
                        Phone = request.Info.Phone,
                        LangId = request.Info.Language,
                        Ip = request.Info.Ip,
                        CountryByIp = request.Info.Country,
                        AffId = Convert.ToInt32(Program.Settings.BrandAffiliateId),
                        BrandId = Program.Settings.BrandBrandId,
                        SecretKey = Program.Settings.BrandAffiliateKey,
                        ProcessId = DateTimeOffset.UtcNow.ToString(),
                        CountryOfRegistration = request.Info.Country,
                    });

                // Failed
                if (registerResult.IsFailed)
                {
                    return FailedMapToGrpc(new Error()
                    {
                        Message = registerResult.FailedResult.Message,
                        Type = ErrorType.Unknown
                    }, ResultCode.Failed);
                }

                // Success
                if (registerResult.SuccessResult.IsSuccessfully())
                {
                    // Success
                    return SuccessMapToGrpc(registerResult.SuccessResult);
                }

                // Success, but software failure
                if ((SimpleTradingResultCode)registerResult.SuccessResult.Status ==
                    SimpleTradingResultCode.UserExists)
                {
                    return FailedMapToGrpc(new Error()
                    {
                        Message = "Registration already exists",
                        Type = ErrorType.AlreadyExist
                    }, ResultCode.Failed);
                }

                if ((SimpleTradingResultCode)registerResult.SuccessResult.Status ==
                    SimpleTradingResultCode.InvalidUserNameOrPassword)
                {
                    return FailedMapToGrpc(new Error()
                    {
                        Message = "Invalid username or password",
                        Type = ErrorType.InvalidUserNameOrPassword
                    }, ResultCode.Failed);
                }

                if ((SimpleTradingResultCode)registerResult.SuccessResult.Status ==
                    SimpleTradingResultCode.PersonalDataNotValid)
                {
                    return FailedMapToGrpc(new Error()
                    {
                        Message = "Registration data not valid",
                        Type = ErrorType.InvalidPersonalData
                    }, ResultCode.Failed);
                }

                if ((SimpleTradingResultCode)registerResult.SuccessResult.Status ==
                    SimpleTradingResultCode.SystemError)
                {
                    return FailedMapToGrpc(new Error()
                    {
                        Message = "Brand Error",
                        Type = ErrorType.Unknown
                    }, ResultCode.Failed);
                }

                return FailedMapToGrpc(new Error()
                {
                    Message = "Unknown Error",
                    Type = ErrorType.Unknown
                }, ResultCode.Failed);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating lead {@context}", request);

                return FailedMapToGrpc(new Error()
                {
                    Message = "Brand response parse error",
                    Type = ErrorType.Unknown
                }, ResultCode.Failed);
            }
        }

        public static RegistrationBridgeResponse SuccessMapToGrpc(RegisterResponse brandRegistrationInfo)
        {
            return new RegistrationBridgeResponse()
            {
                ResultCode = ResultCode.CompletedSuccessfully,
                ResultMessage = EnumExtensions.GetDescription((ResultCode)ResultCode.CompletedSuccessfully),
                RegistrationInfo = new RegisteredLeadInfo()
                {
                    CustomerId = brandRegistrationInfo.TraderId,
                    LoginUrl = brandRegistrationInfo.RedirectUrl,
                    Token = brandRegistrationInfo.Token
                }
            };
        }

        public static RegistrationBridgeResponse FailedMapToGrpc(Error error, ResultCode code)
        {
            return new RegistrationBridgeResponse()
            {
                ResultCode = code,
                ResultMessage = EnumExtensions.GetDescription((ResultCode)code),
                Error = error
            };
        }

        public Task<BridgeCountersResponse> GetBridgeCountersPerPeriodAsync(CountersRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RegistrationsResponse> GetRegistrationsPerPeriodAsync(RegistrationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<DepositsResponse> GetDepositsPerPeriodAsync(DepositsRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
