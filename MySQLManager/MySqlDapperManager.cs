using Dapper;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQLManager
{
    public class MySqlDapperManager : IDisposable
    {
        public static Dictionary<string, SqlManager> sqlManager = new Dictionary<string, SqlManager>();
        MySqlConnection connection = null;
        MySqlTransaction transaction = null;
        private bool disposedValue = false;

        public MySqlDapperManager()
        {
            if (ConnectionStringRepository.MySqlStorage == null || ConnectionStringRepository.MySqlStorage.Count == 0) throw new InvalidOperationException("MySQL 연결 문자열이 설정되지 않았습니다.");

            var connectionString = ConnectionStringRepository.MySqlStorage.First().Value;
            if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("MySQL 연결 문자열이 비어 있습니다.");

            connection = new MySqlConnection(connectionString);
        }
        public MySqlDapperManager(ConnectionStringEnum connectionStringEnum)
        {
            if (ConnectionStringRepository.MySqlStorage == null || ConnectionStringRepository.MySqlStorage.Count == 0) throw new InvalidOperationException("MySQL 연결 문자열이 설정되지 않았습니다.");

            var connectionString = ConnectionStringRepository.MySqlStorage[connectionStringEnum];
            if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("MySQL 연결 문자열이 비어 있습니다.");

            connection = new MySqlConnection(connectionString);
        }
        private static void ErrorSqlLog(string errorMsg, string paramJson, string errorSql)
        {
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine("=========================================");
            logBuilder.AppendLine($"Timestamp : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            logBuilder.AppendLine("Error Message:");
            logBuilder.AppendLine(errorMsg);
            logBuilder.AppendLine("-----------------------------------------");
            logBuilder.AppendLine("SQL Statement:");
            logBuilder.AppendLine(errorSql);
            logBuilder.AppendLine("-----------------------------------------");
            logBuilder.AppendLine("Parameters:");
            logBuilder.AppendLine(paramJson);
            logBuilder.AppendLine("=========================================");

            string log = logBuilder.ToString();
            Console.WriteLine(log);
        }

        public static string GetSqlFromXml(string xmlPath, string sqlId)
        {
            if (sqlManager.ContainsKey(xmlPath) == false) sqlManager[xmlPath] = new SqlManager(xmlPath);

            return sqlManager[xmlPath].GetSql(sqlId);
        }
        public async Task<List<T>> GetQueryAsync<T>(string sql, object param)
        {
            try
            {
                return (await SqlMapper.QueryAsync<T>(connection, sql, param, transaction)).ToList();
            }
            catch (Exception ex)
            {
                ErrorSqlLog(sql, JsonConvert.SerializeObject(param), ex.Message);
                throw ex;
            }
        }

        public async Task<List<T>> GetQueryFromXmlAsync<T>(string xmlPath, string sqlId, object param)
        {
            if (sqlManager.ContainsKey(xmlPath) == false) sqlManager[xmlPath] = new SqlManager(xmlPath);

            return (await this.GetQueryAsync<T>(sqlManager[xmlPath].GetSql(sqlId), param)).ToList();
        }

        public async Task<T> GetQueryFirstAsync<T>(string sql, object param)
        {
            try
            {
                return (await Dapper.SqlMapper.QueryFirstOrDefaultAsync<T>(connection, sql, param, transaction));
            }
            catch (Exception ex)
            {
                ErrorSqlLog(sql, JsonConvert.SerializeObject(param), ex.Message);
                throw ex;
            }
        }

        public async Task<T> GetQueryFirstFromXmlAsync<T>(string xmlPath, string sqlId, object param)
        {
            if (sqlManager.ContainsKey(xmlPath) == false) sqlManager[xmlPath] = new SqlManager(xmlPath);

            return await this.GetQueryFirstAsync<T>(sqlManager[xmlPath].GetSql(sqlId), param);
        }

        public async Task<int> ExecuteAsync(string sql, object param)
        {
            try
            {
                return await Dapper.SqlMapper.ExecuteAsync(connection, sql, param, transaction);
            }
            catch (Exception ex)
            {
                ErrorSqlLog(sql, JsonConvert.SerializeObject(param), ex.Message);
                throw ex;
            }
        }

        public async Task<int> ExecuteFromXmlAsync(string xmlPath, string sqlId, object param)
        {
            if (sqlManager.ContainsKey(xmlPath) == false) sqlManager[xmlPath] = new SqlManager(xmlPath);

            return await this.ExecuteAsync(sqlManager[xmlPath].GetSql(sqlId), param);
        }
        public static async Task<List<T>> GetQueryAsync<T>(ConnectionStringEnum connectionStringEnum, string sql, object param)
        {
            using (var db = new MySqlDapperManager(connectionStringEnum))
            {
                return (await db.GetQueryAsync<T>(sql, param)).ToList();
            }
        }

        public static async Task<List<T>> GetQueryFromXmlAsync<T>(ConnectionStringEnum connectionStringEnum, string xmlPath, string sqlId, object param)
        {
            using (var db = new MySqlDapperManager(connectionStringEnum))
            {
                return (await db.GetQueryFromXmlAsync<T>(xmlPath, sqlId, param)).ToList();
            }
        }

        public static async Task<T> GetQueryFirstAsync<T>(ConnectionStringEnum connectionStringEnum, string sql, object param)
        {
            using (var db = new MySqlDapperManager(connectionStringEnum))
            {
                return await db.GetQueryFirstAsync<T>(sql, param);
            }
        }

        public static async Task<T> GetQueryFirstFromXmlAsync<T>(ConnectionStringEnum connectionStringEnum, string xmlPath, string sqlId, object param)
        {
            using (var db = new MySqlDapperManager(connectionStringEnum))
            {
                return await db.GetQueryFirstFromXmlAsync<T>(xmlPath, sqlId, param);
            }
        }

        public static async Task<int> ExecuteAsync(ConnectionStringEnum connectionStringEnum, string sql, object param)
        {
            using (var db = new MySqlDapperManager(connectionStringEnum))
            {
                return await db.ExecuteAsync(sql, param);
            }
        }

        public static async Task<int> ExecuteFromXmlAsync(ConnectionStringEnum connectionStringEnum, string xmlPath, string sqlId, object param)
        {
            using (var db = new MySqlDapperManager(connectionStringEnum))
            {
                return await db.ExecuteFromXmlAsync(xmlPath, sqlId, param);
            }
        }

        #region Transaction
        public void BeginTransaction()
        {
            if (connection.State == System.Data.ConnectionState.Closed) connection.Open();
            transaction = connection.BeginTransaction();
        }
        public void Rollback()
        {
            transaction.Rollback();
            transaction = null;

            if (connection.State != System.Data.ConnectionState.Closed) connection.Close();
        }
        public void Commit()
        {
            transaction.Commit();
            transaction = null;

            if (connection.State != System.Data.ConnectionState.Closed) connection.Close();
        }
        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    transaction?.Rollback();
                    transaction?.Dispose();

                    connection?.Dispose();
                    connection = null;
                    transaction = null;
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
