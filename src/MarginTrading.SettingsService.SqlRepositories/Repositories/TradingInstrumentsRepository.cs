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

        private readonly string _connectionString;
        private readonly ILog _log;
        
        public TradingInstrumentsRepository(string connectionString, ILog log)
        {
            _log = log;
            _connectionString = connectionString;

            connectionString.InitializeSqlObject("TradingInstruments.sql", log);
            connectionString.InitializeSqlObject("CompileAndGetTradingInstrument.sql", log);
            connectionString.InitializeSqlObject("CompileAndGetAllTradingInstruments.sql", log);
        }

        public async Task<IReadOnlyList<ITradingInstrument>> GetByTradingConditionAsync(string tradingConditionId, 
            bool raw = false)
        {
            var data = await GetByPagesAsync(tradingConditionId, raw: raw);
            return data.Contents;
        }

        public async Task<PaginatedResponse<ITradingInstrument>> GetByPagesAsync(string tradingConditionId = null,
            int? skip = null, int? take = null, bool sortAscending = true, bool raw = false)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                List<TradingInstrumentEntity> objects;
                if (raw)
                {
                    var whereClause = "WHERE 1=1 "
                                      + (string.IsNullOrWhiteSpace(tradingConditionId) ? "" : " AND TradingConditionId=@tradingConditionId");
                    var orderDirection = sortAscending ? "ASC" : "DESC";
                    var orderClause = $"ORDER BY TradingConditionId {orderDirection}, Instrument {orderDirection}";
                    var paginationClause = $"OFFSET {skip ?? 0} ROWS FETCH NEXT {take ?? int.MaxValue} ROWS ONLY";
                    var gridReader = await conn.QueryMultipleAsync(
                        $"SELECT * FROM {TableName} {whereClause} {orderClause} {paginationClause}; SELECT COUNT(*) FROM {TableName} {whereClause}",
                        new {tradingConditionId});

                    objects = (await gridReader.ReadAsync<TradingInstrumentEntity>()).ToList();
                    var totalRows = await gridReader.ReadSingleAsync<int>();
                    objects.ForEach(x => x.TotalRows = totalRows);
                }
                else
                {
                    objects = (await conn.QueryAsync<TradingInstrumentEntity>(
                        GetAllByTradingConditionProcName,
                        new
                        {
                            TradingConditionId = tradingConditionId,
                            Skip = skip ?? 0,
                            Take = take ?? int.MaxValue,
                            SortAscending = sortAscending ? 1 : 0,
                        },
                        commandType: CommandType.StoredProcedure)).ToList();
                }
                
                var tradingInstruments = objects.Select(TradingInstrument.Create)
                    .ToList();
                
                return new PaginatedResponse<ITradingInstrument>(
                    contents: tradingInstruments,
                    start: skip ?? 0,
                    size: tradingInstruments.Count,
                    totalSize: objects.GetTotalRows()
                );
            }
        }

        public async Task<ITradingInstrument> GetAsync(string assetPairId, string tradingConditionId, bool raw = false)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                IEnumerable<TradingInstrumentEntity> objects;
                if (raw)
                {
                    objects = await conn.QueryAsync<TradingInstrumentEntity>(
                        $"SELECT * FROM {TableName} WHERE TradingConditionId=@tradingConditionId AND Instrument=@assetPairId",
                        new
                        {
                            tradingConditionId = tradingConditionId, 
                            assetPairId = assetPairId,
                        });
                }
                else
                {
                    objects = await conn.QueryAsync<TradingInstrumentEntity>(
                        GetProcName,
                        new
                        {
                            TradingConditionId = tradingConditionId,
                            TradingInstrumentId = assetPairId,
                        },
                        commandType: CommandType.StoredProcedure);
                }

                return objects.Select(TradingInstrument.Create).FirstOrDefault();
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
                        TradingInstrumentEntity.Create(tradingInstrument));
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
                    TradingInstrumentEntity.Create(tradingInstrument));
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
                            objectsToAdd.Select(TradingInstrumentEntity.Create), 
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
            var entities = items.Select(TradingInstrumentEntity.Create).ToList();
            
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