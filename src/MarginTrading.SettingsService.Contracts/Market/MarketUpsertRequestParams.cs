using MarginTrading.SettingsService.Contracts.Common;

namespace MarginTrading.SettingsService.Contracts.Market
{
    public class MarketUpsertRequestParams
    {
        public MarketContract Market { get; set; }
        
        public TraceableRequestParams Traceability { get; set; }
    }
}