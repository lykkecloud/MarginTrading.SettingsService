using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.SettingsService.Contracts.Common;
using MarginTrading.SettingsService.Contracts.Market;
using Refit;

namespace MarginTrading.SettingsService.Contracts
{
    [PublicAPI]
    public interface IMarketsApi
    {
        [Get("/api/markets")]
        Task<List<MarketContract>> List();


        [Post("/api/markets")]
        Task<MarketContract> Insert([NotNull] [Body] MarketUpsertRequestParams @params);

        
        [ItemCanBeNull]
        [Get("/api/markets/{marketId}")]
        Task<MarketContract> Get([NotNull] string marketId);


        [Put("/api/markets/{marketId}")]
        Task<MarketContract> Update([NotNull] string marketId, [NotNull] [Body] MarketUpsertRequestParams @params);


        [Delete("/api/markets/{marketId}")]
        Task Delete([NotNull] string marketId, [NotNull] [Body] TraceableRequestParams @params);

    }
}
