using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.SettingsService.Contracts.Common;
using MarginTrading.SettingsService.Contracts.TradingConditions;
using MarginTrading.SettingsService.Contracts.TradingInstruments;
using Refit;

namespace MarginTrading.SettingsService.Contracts
{
    [PublicAPI]
    public interface ITradingInstrumentsApi
    {
        [Get("/api/tradingInstruments")]
        Task<List<TradingInstrumentContract>> List([Query, CanBeNull] string tradingConditionId);


        [Post("/api/tradingInstruments")]
        Task<TradingInstrumentContract> Insert([NotNull, Body] TradingInstrumentUpsertRequestParams @params);


        /// <summary>
        /// Assign trading instrument to a trading condition with default values
        /// </summary>
        [Post("/api/tradingInstruments/{tradingConditionId}")]
        Task<List<TradingInstrumentContract>> AssignCollection(
            [NotNull] string tradingConditionId,
            [NotNull, Body] TradingInstrumentAssignCollectionRequestParams @params);
        

        [ItemCanBeNull]
        [Get("/api/tradingInstruments/{tradingConditionId}/{assetPairId}")]
        Task<TradingInstrumentContract> Get(
            [NotNull] string tradingConditionId,
            [NotNull] string assetPairId);


        [Put("/api/tradingInstruments/{tradingConditionId}/{assetPairId}")]
        Task<TradingInstrumentContract> Update(
            [NotNull] string tradingConditionId,
            [NotNull] string assetPairId,
            [NotNull, Body] TradingInstrumentUpsertRequestParams @params);


        [Delete("/api/tradingInstruments/{tradingConditionId}/{assetPairId}")]
        Task Delete(
            [NotNull] string tradingConditionId,
            [NotNull] string assetPairId,
            [NotNull, Body] TraceableRequestParams @params);

    }
}
