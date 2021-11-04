using Autofac;
using MarketingBox.Integration.SimpleTrading.Bridge.Services.Integrations;

namespace MarketingBox.Integration.SimpleTrading.Bridge.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SimpleTradingHttpClient>()
                .As<ISimpleTradingHttpClient>()
                .WithParameter("baseUrl", Program.ReloadedSettings(e => e.BrandUrl).Invoke())
                .SingleInstance();
        }
    }
}
