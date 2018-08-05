using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.SettingsService.Contracts.Common;
using MarginTrading.SettingsService.Contracts.Market;
using Refit;

namespace MarginTrading.SettingsService.Contracts
{
    /// <summary>
    /// Markets management
    /// </summary>
    [PublicAPI]
    public interface IMarketsApi
    {
        /// <summary>
        /// Get the list of Markets
        /// </summary>
        [Get("/api/markets")]
        Task<List<MarketContract>> List();

        /// <summary>
        /// Create new market
        /// </summary>
        [Post("/api/markets")]
        Task<MarketContract> Insert([NotNull] [Body] MarketUpsertRequestParams @params);

        /// <summary>
        /// Get the market
        /// </summary>
        [ItemCanBeNull]
        [Get("/api/markets/{marketId}")]
        Task<MarketContract> Get([NotNull] string marketId);

        /// <summary>
        /// Update the market
        /// </summary>
        [Put("/api/markets/{marketId}")]
        Task<MarketContract> Update([NotNull] string marketId, [NotNull] [Body] MarketUpsertRequestParams @params);

        /// <summary>
        /// Delete the market
        /// </summary>
        [Delete("/api/markets/{marketId}")]
        Task Delete([NotNull] string marketId, [NotNull] [Body] TraceableRequestParams @params);

    }
}
