using MarginTrading.SettingsService.Contracts.Common;

namespace MarginTrading.SettingsService.Contracts.AssetPair
{
    public class AssetPairUpsertRequestParams
    {
        public AssetPairContract AssetPair { get; set; }
        
        public TraceableRequestParams Traceability { get; set; }
    }
}