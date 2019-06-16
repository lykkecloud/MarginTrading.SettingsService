using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.Log;
using Dapper;
using MarginTrading.SettingsService.Core;
using MarginTrading.SettingsService.Core.Domain;
using MarginTrading.SettingsService.Core.Helpers;
using MarginTrading.SettingsService.Core.Interfaces;
using MarginTrading.SettingsService.Core.Services;
using MarginTrading.SettingsService.Core.Settings;
using MarginTrading.SettingsService.SqlRepositories.Entities;
using MarginTrading.SettingsService.StorageInterfaces.Repositories;

namespace MarginTrading.SettingsService.SqlRepositories.Repositories
{
    public class TradingInstrumentsRepository : ITradingInstrumentsRepository
    {
        private const string TableName = "TradingInstruments";
        private const string GetProcName = "CompileAndGetTradingInstrument";
        private const string GetAllByTradingConditionProcName = "CompileAndGetAllTradingInstruments";
        
        private static Type DataType => typeof(ITradingInstrument);
        private static readonly string GetColumns = "[" + string.Join("],[", DataType.GetProperties().Select(x => x.Name)) + "]";
        private static readonly string GetFields = string.Join(",", DataType.GetProperties().Select(x => "@" + x.Name));
        private static readonly string GetUpdateClause = string.Join(",",
            DataType.GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        private readonly IConvertService _convertService;
        private readonly string _connectionString;
        private readonly ILog _log;
        
        public TradingInstrumentsRepository(IConvertService convertService, string connectionString, ILog log)
        {
            _convertService = convertService;
            _log = log;
            _connectionString = connectionString;

            var projectPath = GetType().Assembly.GetName().Name;
            connectionString.InitializeSqlObject("TableTradingInstruments.sql", projectPath, log);
            connectionString.InitializeSqlObject("ProcGetTradingInstrument.sql", projectPath, log);
            connectionString.InitializeSqlObject("ProcGetAllTradingInstruments.sql", projectPath, log);
        }

        public async Task<IReadOnlyList<ITradingInstrument>> GetAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var tradingConditions = await conn.QueryAsync<string>(
                    $"SELECT Id FROM {TradingConditionsRepository.TableName}");

                var objects = new List<TradingInstrumentEntity>();
                foreach (var tradingCondition in tradingConditions)
                {
                    var data = await conn.QueryAsync<TradingInstrumentEntity>(
                        GetAllByTradingConditionProcName,
                        new {TradingConditionId = tradingCondition},
                        commandType: CommandType.StoredProcedure);
                    objects.AddRange(data);
                }
                
                return objects.Select(_convertService.Convert<TradingInstrumentEntity, TradingInstrument>).ToList();
            }
        }

        public async Task<IReadOnlyList<ITradingInstrument>> GetByTradingConditionAsync(string tradingConditionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var objects = await conn.QueryAsync<TradingInstrumentEntity>(
                    GetAllByTradingConditionProcName,
                    new {TradingConditionId = tradingConditionId},
                    commandType: CommandType.StoredProcedure);
                
                return objects.Select(_convertService.Convert<TradingInstrumentEntity, TradingInstrument>).ToList();
            }
        }

        /// <summary>
        /// It's not paginated implementation - it just takes all, and filter in-mem.
        /// </summary>
        public async Task<PaginatedResponse<ITradingInstrument>> GetByPagesAsync(string tradingConditionId = null,
            int? skip = null, int? take = null)
        {
            var all = tradingConditionId == null
                ? await GetAsync()
                : await GetByTradingConditionAsync(tradingConditionId);

            var tradingInstruments = all.Skip(skip ?? 0).Take(PaginationHelper.GetTake(take)).ToList();

            return new PaginatedResponse<ITradingInstrument>(
                contents: tradingInstruments,
                start: skip ?? 0,
                size: tradingInstruments.Count,
                totalSize: all.Count
            );
        }

        public async Task<ITradingInstrument> GetAsync(string assetPairId, string tradingConditionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var objects = await conn.QueryAsync<TradingInstrumentEntity>(
                    GetProcName,
                    new {TradingConditionId = tradingConditionId},
                    commandType: CommandType.StoredProcedure);
                
                return objects.Select(_convertService.Convert<TradingInstrumentEntity, TradingInstrument>).FirstOrDefault();
            }
        }

        public async Task<bool> TryInsertAsync(ITradingInstrument tradingInstrument)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                try
                {
                    if (null != await conn.QueryFirstOrDefaultAsync<TradingInstrumentEntity>(
                            $"SELECT * FROM {TableName} WHERE TradingConditionId=@tradingConditionId AND Instrument=@assetPairId",
                            new
                            {
                                tradingConditionId = tradingInstrument.TradingConditionId, 
                                assetPairId = tradingInstrument.Instrument
                            }))
                    {
                        return false;
                    }

                    await conn.ExecuteAsync(
                        $"insert into {TableName} ({GetColumns}) values ({GetFields})",
                        _convertService.Convert<ITradingInstrument, TradingInstrumentEntity>(tradingInstrument));
                }
                catch (Exception ex)
                {
                    _log?.WriteWarningAsync(nameof(AssetPairsRepository), nameof(TryInsertAsync),
                        $"Failed to insert a trading instrument with assetPairId {tradingInstrument.Instrument} and tradingConditionId {tradingInstrument.TradingConditionId}", ex);
                    return false;
                }

                return true;
            }
        }

        public async Task UpdateAsync(ITradingInstrument tradingInstrument)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.ExecuteAsync(
                    $"update {TableName} set {GetUpdateClause} where TradingConditionId=@TradingConditionId AND Instrument=@Instrument", 
                    _convertService.Convert<ITradingInstrument, TradingInstrumentEntity>(tradingInstrument));
            }
        }

        public async Task DeleteAsync(string assetPairId, string tradingConditionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.ExecuteAsync(
                    $"DELETE {TableName} WHERE TradingConditionId=@tradingConditionId AND Instrument=@assetPairId",
                    new
                    {
                        tradingConditionId,
                        assetPairId
                    });
            }
        }

        public async Task<IEnumerable<ITradingInstrument>> CreateDefaultTradingInstruments(string tradingConditionId, 
            IEnumerable<string> assetPairsIds, DefaultTradingInstrumentSettings defaults)
        {
            var objectsToAdd = assetPairsIds.Select(x => new TradingInstrument
            (
                tradingConditionId,
                x,
                defaults.LeverageInit,
                defaults.LeverageMaintenance,
                defaults.SwapLong,
                defaults.SwapShort,
                defaults.Delta,
                defaults.DealMinLimit,
                defaults.DealMaxLimit,
                defaults.PositionLimit,
                true,
                defaults.LiquidationThreshold,
                defaults.OvernightMarginMultiplier,
                defaults.CommissionRate,
                defaults.CommissionMin,
                defaults.CommissionMax,
                defaults.CommissionCurrency
            )).ToList();
            
            
                using (var conn = new SqlConnection(_connectionString))
                {
                    SqlTransaction transaction = null;
                    try
                    {
                        transaction = conn.BeginTransaction();
                        
                        await conn.ExecuteAsync(
                            $"insert into {TableName} ({GetColumns}) values ({GetFields})", 
                            objectsToAdd.Select(_convertService.Convert<TradingInstrument, TradingInstrumentEntity>), 
                            transaction);
                        
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction?.Rollback();
                        await _log.WriteErrorAsync(nameof(TradingInstrumentsRepository),
                            nameof(CreateDefaultTradingInstruments), "Failed to create default trading instruments", ex);
                    }
                }
           

            return objectsToAdd;
        }

        public async Task<List<ITradingInstrument>> UpdateBatchAsync(string tradingConditionId,
            List<TradingInstrument> items)
        {
            var entities = items.Select(_convertService.Convert<TradingInstrument, TradingInstrumentEntity>).ToList();
            
            using (var conn = new SqlConnection(_connectionString))
            {
                SqlTransaction transaction = null;
                
                try
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        await conn.OpenAsync();
                    }
                    
                    transaction = conn.BeginTransaction();

                    var ids = string.Join(",", entities.Select(x => $"'{x.Instrument}'"));
                    var idsCond = $"TradingConditionId = @tradingConditionId and Instrument IN ({ids})";

                    if (await conn.ExecuteScalarAsync<int>(
                            $"SELECT COUNT(*) FROM {TableName} WITH (UPDLOCK) WHERE {idsCond}",
                            new {tradingConditionId},
                            transaction) != entities.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(entities),
                            "One of trading instruments does not exist");
                    }

                    await conn.ExecuteAsync(
                        $"update {TableName} set {GetUpdateClause} " +
                        "where TradingConditionId=@TradingConditionId AND Instrument=@Instrument",
                        entities,
                        transaction);

                    var updated = await conn.QueryAsync<TradingInstrumentEntity>(
                        $"SELECT * FROM {TableName} WITH (UPDLOCK) WHERE {idsCond}",
                        new {tradingConditionId},
                        transaction);

                    transaction.Commit();

                    return updated.Cast<ITradingInstrument>().ToList();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    await _log.WriteErrorAsync(nameof(AssetPairsRepository),
                        nameof(UpdateBatchAsync), $"Failed to perform batch transaction: {ex.Message}", ex);
                    
                    return null;
                }
            }
        }
    }
}