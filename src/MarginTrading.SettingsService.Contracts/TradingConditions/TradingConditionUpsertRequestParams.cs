using MarginTrading.SettingsService.Contracts.Common;

namespace MarginTrading.SettingsService.Contracts.TradingConditions
{
    public class TradingConditionUpsertRequestParams
    {
        public TradingConditionContract TradingCondition { get; set; }
        
        public TraceableRequestParams Traceability { get; set; }
    }
}