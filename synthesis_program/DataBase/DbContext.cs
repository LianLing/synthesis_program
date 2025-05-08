using MySqlX.XDevAPI;
using SqlSugar;
using System;
using System.Configuration;

namespace synthesis_program.DataBase
{
    public class DbContext :IDisposable
    {
        private SqlSugarClient _client;
        public SqlSugarClient Instance => _client ?? (_client = new SqlSugarClient(
            new ConnectionConfig()
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["MySQL"].ConnectionString,
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            }));


        public void Dispose() 
        {
            _client?.Close();  // 关闭连接
            _client?.Dispose();
        }
    }
}