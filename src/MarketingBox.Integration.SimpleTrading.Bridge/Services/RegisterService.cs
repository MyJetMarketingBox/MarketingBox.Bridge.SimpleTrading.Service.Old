using System;
using System.Threading.Tasks;
using MarketingBox.Integration.Bridge.Client;
using MarketingBox.Integration.Service.Grpc.Models.Common;
using MarketingBox.Integration.Service.Grpc.Models.Registrations.Contracts.Bridge;
using MarketingBox.Integration.SimpleTrading.Bridge.Domain.Extensions;
using MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations;
using MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations.Contracts.Enums;
using MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations.Contracts.Requests;
using MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations.Contracts.Responses;
using MarketingBox.Integration.SimpleTrading.Bridge.Settings;
using Microsoft.Extensions.Logging;

namespace MarketingBox.Integration.SimpleTrading.Bridge.Services
{
    public class RegisterService : IBridgeService
    {
        private readonly ILogger<RegisterService> _logger;
        private readonly ISimpleTradingHttpClient _simpleTradingHttpClient;
        private readonly SettingsModel _settingsModel;

        public RegisterService(ILogger<RegisterService> logger,
            ISimpleTradingHttpClient simpleTradingHttpClient, SettingsModel settingsModel)
        {
            _logger = logger;
            _simpleTradingHttpClient = simpleTradingHttpClient;
            _settingsModel = settingsModel;
        }

        public async Task<RegistrationResponse> SendRegistrationAsync(RegistrationRequest request)
        {
            _logger.LogInformation("Creating new LeadInfo {@context}", request);

            var brandRequest = MapToApi(request, _settingsModel.BrandBrandId, 
                Convert.ToInt32(_settingsModel.BrandAffiliateId), _settingsModel.BrandAffiliateKey,
                DateTimeOffset.UtcNow.ToString());

            try
            {
                return await RegisterExternalCustomerAsync(brandRequest);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating lead {@context}", request);

                return FailedMapToGrpc(new Error()
                {
                    Message = "Brand response error",
                    Type = ErrorType.Unknown
                }, ResultCode.Failed);
            }
        }

        public async Task<RegistrationResponse> RegisterExternalCustomerAsync(RegisterRequest brandRequest)
        {
            var registerResult =
                await _simpleTradingHttpClient.RegisterTraderAsync(brandRequest);

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
            if (registerResult.SuccessResult.IsSuccessfully() &&
                (SimpleTradingResultCode)registerResult.SuccessResult.Status ==
                SimpleTradingResultCode.Ok)
            {
                // Success
                return SuccessMapToGrpc(registerResult.SuccessResult);
            }

            // Success, but software failure
            if ((SimpleTradingResultCode) registerResult.SuccessResult.Status ==
                SimpleTradingResultCode.UserExists)
            {
                return FailedMapToGrpc(new Error()
                {
                    Message = "Registration already exists",
                    Type = ErrorType.AlreadyExist
                }, ResultCode.Failed);
            }

            if ((SimpleTradingResultCode) registerResult.SuccessResult.Status ==
                SimpleTradingResultCode.InvalidUserNameOrPassword)
            {
                return FailedMapToGrpc(new Error()
                {
                    Message = "Invalid username or password",
                    Type = ErrorType.InvalidUserNameOrPassword
                }, ResultCode.Failed);
            }

            if ((SimpleTradingResultCode) registerResult.SuccessResult.Status ==
                SimpleTradingResultCode.PersonalDataNotValid)
            {
                return FailedMapToGrpc(new Error()
                {
                    Message = "Registration data not valid",
                    Type = ErrorType.InvalidPersonalData
                }, ResultCode.Failed);
            }

            if ((SimpleTradingResultCode) registerResult.SuccessResult.Status ==
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

        private RegisterRequest MapToApi(RegistrationRequest request,
            string authBrandId, int authAffId, string authAffApiKey, string requestId)
        {
            return new RegisterRequest()
            {
                FirstName = request.Info.FirstName,
                LastName = request.Info.LastName,
                Password = request.Info.Password,
                Email = request.Info.Email,
                Phone = request.Info.Phone,
                LangId = request.Info.Language,
                Ip = request.Info.Ip,
                CountryByIp = request.Info.Country,
                AffId = authAffId,
                BrandId = authBrandId,
                SecretKey = authAffApiKey,
                ProcessId = requestId,
                CountryOfRegistration = request.Info.Country,
            };
        }

        public static RegistrationResponse SuccessMapToGrpc(RegisterResponse brandRegistrationInfo)
        {
            return new RegistrationResponse()
            {
                ResultCode = ResultCode.CompletedSuccessfully,
                ResultMessage = EnumExtensions.GetDescription((ResultCode)ResultCode.CompletedSuccessfully),
                CustomerInfo = new Service.Grpc.Models.Registrations.CustomerInfo()
                {
                    CustomerId = brandRegistrationInfo.TraderId,
                    LoginUrl = brandRegistrationInfo.RedirectUrl,
                    Token = brandRegistrationInfo.Token
                }
            };
        }

        public static RegistrationResponse FailedMapToGrpc(Error error, ResultCode code)
        {
            return new RegistrationResponse()
            {
                ResultCode = code,
                ResultMessage = EnumExtensions.GetDescription((ResultCode)code),
                Error = error
            };
        }

        public Task<ReportingResponse> GetRegistrationsPerPeriodAsync(ReportingRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
