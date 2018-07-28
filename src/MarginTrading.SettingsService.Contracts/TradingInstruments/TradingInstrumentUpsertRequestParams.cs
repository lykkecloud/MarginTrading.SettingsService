using MarginTrading.SettingsService.Contracts.Common;

namespace MarginTrading.SettingsService.Contracts.TradingInstruments
{
    public class TradingInstrumentUpsertRequestParams
    {
        public TradingInstrumentContract TradingInstrument { get; set; }
        
        public TraceableRequestParams Traceability { get; set; }
    }
}