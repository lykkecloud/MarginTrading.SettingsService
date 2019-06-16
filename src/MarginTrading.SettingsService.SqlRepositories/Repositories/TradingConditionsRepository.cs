using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Dapper;
using MarginTrading.SettingsService.Core.Domain;
using MarginTrading.SettingsService.Core.Interfaces;
using MarginTrading.SettingsService.Core.Services;
using MarginTrading.SettingsService.SqlRepositories.Entities;
using MarginTrading.SettingsService.StorageInterfaces.Repositories;

namespace MarginTrading.SettingsService.SqlRepositories.Repositories
{
    public class TradingConditionsRepository : ITradingConditionsRepository
    {
        public const string TableName = "TradingConditions";
        private const string InsertProcName = "InsertTradingCondition";
        private const string UpdateProcName = "UpdateTradingCondition";
        
        private static Type DataType => typeof(ITradingCondition);
        private static readonly string GetColumns = "[" + string.Join("],[", DataType.GetProperties().Select(x => x.Name)) + "]";
        private static readonly string GetFields = string.Join(",", DataType.GetProperties().Select(x => "@" + x.Name));
        private static readonly string GetUpdateClause = string.Join(",",
            DataType.GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        private readonly IConvertService _convertService;
        private readonly string _connectionString;
        private readonly ILog _log;
        
        public TradingConditionsRepository(IConvertService convertService, string connectionString, ILog log)
        {
            _convertService = convertService;
            _log = log;
            _connectionString = connectionString;
            
            var projectPath = GetType().Assembly.GetName().Name;
            connectionString.InitializeSqlObject("TableTradingConditions.sql", projectPath, log);
            connectionString.InitializeSqlObject("ProcInsertTradingCondition.sql", projectPath, log);
            connectionString.InitializeSqlObject("ProcUpdateTradingCondition.sql", projectPath, log);
        }

        public async Task<IReadOnlyList<ITradingCondition>> GetAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var objects = await conn.QueryAsync<TradingConditionEntity>($"SELECT * FROM {TableName}");
                
                return objects.Select(_convertService.Convert<TradingConditionEntity, TradingCondition>).ToList();
            }
        }

        public async Task<ITradingCondition> GetAsync(string tradingConditionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var objects = await conn.QueryAsync<TradingConditionEntity>(
                    $"SELECT * FROM {TableName} WHERE Id=@id", new {id = tradingConditionId});
                
                return objects.Select(_convertService.Convert<TradingConditionEntity, TradingCondition>).FirstOrDefault();
            }
        }

        public async Task<IReadOnlyList<ITradingCondition>> GetDefaultAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var objects = await conn.QueryAsync<TradingConditionEntity>(
                    $"SELECT * FROM {TableName} WHERE IsDefault = 1");
                
                return objects.Select(_convertService.Convert<TradingConditionEntity, TradingCondition>).ToList();
            }
        }

        public async Task<bool> TryInsertAsync(ITradingCondition tradingCondition)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                try
                {
                    await conn.ExecuteAsync(InsertProcName, tradingCondition,
                        commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    _log?.WriteWarningAsync(nameof(AssetPairsRepository), nameof(TryInsertAsync),
                        $"Failed to insert a trading condition with Id {tradingCondition.Id}", ex).Wait();
                    return false;
                }

                return true;
            }
        }

        public async Task UpdateAsync(ITradingCondition tradingCondition)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.ExecuteAsync(UpdateProcName, tradingCondition,
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}