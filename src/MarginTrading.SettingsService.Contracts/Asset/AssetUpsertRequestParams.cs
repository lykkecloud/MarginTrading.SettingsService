using MarginTrading.SettingsService.Contracts.Common;

namespace MarginTrading.SettingsService.Contracts.Asset
{
    public class AssetUpsertRequestParams
    {
        public AssetContract Asset { get; set; }
        
        public TraceableRequestParams Traceability { get; set; }
    }
}