using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.SettingsService.Contracts.TradingConditions;
using Refit;

namespace MarginTrading.SettingsService.Contracts
{
    [PublicAPI]
    public interface ITradingConditionsApi
    {
        [Get("/api/tradingConditions")]
        Task<List<TradingConditionContract>> List([Query] bool? isDefault = null);


        [Post("/api/tradingConditions")]
        Task<TradingConditionContract> Insert([NotNull] [Body] TradingConditionUpsertRequestParams @params);


        [ItemCanBeNull]
        [Get("/api/tradingConditions/{tradingConditionId}")]
        Task<TradingConditionContract> Get([NotNull] string tradingConditionId);
        
        
        [Put("/api/tradingConditions/{tradingConditionId}")]
        Task<TradingConditionContract> Update(
            [NotNull] string tradingConditionId,
            [NotNull] [Body] TradingConditionUpsertRequestParams @params);

    }
}
