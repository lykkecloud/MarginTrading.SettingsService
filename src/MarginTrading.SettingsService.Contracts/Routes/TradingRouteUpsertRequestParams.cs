using MarginTrading.SettingsService.Contracts.Common;

namespace MarginTrading.SettingsService.Contracts.Routes
{
    public class TradingRouteUpsertRequestParams
    {
        public MatchingEngineRouteContract TradingRoute { get; set; }
        
        public TraceableRequestParams Traceability { get; set; }
    }
}