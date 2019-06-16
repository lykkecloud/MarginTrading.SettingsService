using System;
using System.Data;
using System.Data.SqlClient;
using Common.Log;
using Dapper;
using MarginTrading.SettingsService.Core.Extensions;
using MarginTrading.SettingsService.Core.Helpers;

namespace MarginTrading.SettingsService.SqlRepositories
{
    public static class SqlExtensions
    {   
        public static void InitializeSqlObject(this string connectionString, string scriptFileName, string projectPath, 
            ILog log = null)
        {
            var creationScript = FileExtensions.ReadFromFile(projectPath, scriptFileName);
            
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Execute(creationScript);
                }
                catch (Exception ex)
                {
                    log?.WriteErrorAsync(projectPath, nameof(InitializeSqlObject), 
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
    }
}