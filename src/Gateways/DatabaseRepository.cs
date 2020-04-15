using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;

namespace Gateways
{
    public class DatabaseRepository : IDatabaseRepository
    {
        private SqlConnection _cominoDbConnection;

        public DatabaseRepository()
        {
            _cominoDbConnection = new SqlConnection(Environment.GetEnvironmentVariable("cominodbconnstr"));
        }

        public IEnumerable<W2BatchPrintRow> QueryBatchPrint(string query)
        {
            return _cominoDbConnection.Query<W2BatchPrintRow>(query);
        }
    }

    public class W2BatchPrintRow
    {
        public string DocumentNumber { get; set; }
    }
}