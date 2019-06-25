using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.SettingsService.Core.Domain;
using MarginTrading.SettingsService.Core.Interfaces;
using MarginTrading.SettingsService.Core.Settings;

namespace MarginTrading.SettingsService.StorageInterfaces.Repositories
{
    public interface ITradingInstrumentsRepository
    {
        Task<IReadOnlyList<ITradingInstrument>> GetByTradingConditionAsync(string tradingConditionId = null, 
            bool raw = false);
        Task<PaginatedResponse<ITradingInstrument>> GetByPagesAsync(string tradingConditionId = null,
            int? skip = null, int? take = null, bool sortAscending = true, bool raw = false);
        Task<ITradingInstrument> GetAsync(string assetPairId, string tradingConditionId, bool raw = false);
        Task<bool> TryInsertAsync(ITradingInstrument tradingInstrument);
        Task UpdateAsync(ITradingInstrument tradingInstrument);
        Task DeleteAsync(string assetPairId, string tradingConditionId);
        
        Task<IEnumerable<ITradingInstrument>> CreateDefaultTradingInstruments(string tradingConditionId,
            IEnumerable<string> assetPairsIds, DefaultTradingInstrumentSettings defaults);

        Task<List<ITradingInstrument>> UpdateBatchAsync(string tradingConditionId, List<TradingInstrument> items);
    }
}
