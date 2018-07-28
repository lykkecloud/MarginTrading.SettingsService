using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.SettingsService.Contracts.Asset;
using MarginTrading.SettingsService.Contracts.Common;
using Refit;

namespace MarginTrading.SettingsService.Contracts
{
    [PublicAPI]
    public interface IAssetsApi
    {
        [Get("/api/assets")]
        Task<List<AssetContract>> List();


        [Post("/api/assets")]
        Task<AssetContract> Insert([NotNull] [Body] AssetUpsertRequestParams @params);

        
        [ItemCanBeNull]
        [Get("/api/assets/{assetId}")]
        Task<AssetContract> Get([NotNull] string assetId);


        [Put("/api/assets/{assetId}")]
        Task<AssetContract> Update([NotNull] string assetId, [NotNull] [Body] AssetUpsertRequestParams @params);


        [Delete("/api/assets/{assetId}")]
        Task Delete([NotNull] string assetId, [NotNull] [Body] TraceableRequestParams @params);

    }
}
