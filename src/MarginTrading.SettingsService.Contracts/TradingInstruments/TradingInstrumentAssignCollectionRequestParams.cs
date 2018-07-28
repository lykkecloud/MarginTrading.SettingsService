using MarginTrading.SettingsService.Contracts.Common;

namespace MarginTrading.SettingsService.Contracts.TradingInstruments
{
    public class TradingInstrumentAssignCollectionRequestParams
    {
        public string[] Instruments { get; set; }
        
        public TraceableRequestParams Traceability { get; set; }
    }
}