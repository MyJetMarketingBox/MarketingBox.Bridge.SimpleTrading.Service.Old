using MyYamlParser;

namespace MarketingBox.Bridge.SimpleTrading.Service.Settings
{
    public class SettingsModel
    {
        [YamlProperty("MarketingBoxIntegrationSimpleTradingBridge.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("MarketingBoxIntegrationSimpleTradingBridge.JaegerUrl")]
        public string JaegerUrl { get; set; }

        [YamlProperty("MarketingBoxIntegrationSimpleTradingBridge.Brand.Url")]
        public string BrandUrl { get; set; }

        [YamlProperty("MarketingBoxIntegrationSimpleTradingBridge.Brand.AffiliateId")]
        public string BrandAffiliateId { get; set; }

        [YamlProperty("MarketingBoxIntegrationSimpleTradingBridge.Brand.BrandId")]
        public string BrandBrandId { get; set; }

        [YamlProperty("MarketingBoxIntegrationSimpleTradingBridge.Brand.AffiliateKey")]
        public string BrandAffiliateKey { get; set; }
    }
}
