using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Common.Log;
using Dapper;
using MarginTrading.SettingsService.Core.Extensions;
using MarginTrading.SettingsService.Core.Helpers;

namespace MarginTrading.SettingsService.SqlRepositories
{
    public static class SqlExtensions
    {   
        public static void InitializeSqlObject(this string connectionString, string scriptFileName, ILog log = null)
        {
            var creationScript = FileExtensions.ReadFromFile(scriptFileName);
            
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Execute(creationScript);
                }
                catch (Exception ex)
                {
                    log?.WriteErrorAsync(typeof(SqlExtensions).FullName, nameof(InitializeSqlObject), 
                        scriptFileName, ex).Wait();
                    throw;
                }
            }
        }
        
        [Obsolete("Use InitializeSqlObject with a script in the file.")]
        public static void CreateTableIfDoesntExists(this IDbConnection connection, string createQuery,
            string tableName)
        {
            connection.Open();
            try
            {
                // Check if table exists
                connection.ExecuteScalar($"select top 1 * from {tableName}");
            }
            catch (SqlException)
            {
                // Create table
                var query = string.Format(createQuery, tableName);
                connection.Query(query);
            }
            finally
            {
                connection.Close();
            }
        }

        public static int GetTotalRows(this IEnumerable<dynamic> data)
        {
            return data?.FirstOrDefault()?.TotalRows ?? 0;
        }
    }
}