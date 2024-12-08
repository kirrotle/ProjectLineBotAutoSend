using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;
using Dapper;
using LineBotAutoSend;

namespace Project.Dac
{
    public class _Dac
    {
        private string _connectionString;
        private SqlConnection _conn;

        public _Dac()
        {
            IConfigurationRoot config = Program.Builder();
            _connectionString = config.GetConnectionString("Dev") ?? "";
        }

        protected List<T> ExecuteQuery<T>(string sqlStr,object? paramObj = null)
        {
            Connect();

            List<T> result;
            if (paramObj == null)
                result = _conn.Query<T>(sqlStr).ToList() ?? new List<T>();
            else
                result = _conn.Query<T>(sqlStr,paramObj).ToList() ?? new List<T>();

            Close();
            return result;
        }

        private void Connect()
        {
            _conn = new SqlConnection(_connectionString);
            _conn.Open();
        }

        private void Close()
        {
            _conn.Close();
            _conn.Dispose();
        }

    }
}
