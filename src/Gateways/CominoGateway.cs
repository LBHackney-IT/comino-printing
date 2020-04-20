using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Usecases.Domain;
using UseCases.GatewayInterfaces;

namespace Gateways
{
    public class CominoGateway : ICominoGateway
    {
        private IDbConnection _database;

        public CominoGateway(IDbConnection database)
        {
            _database = database;
        }

        public List<DocumentDetails> GetDocumentsAfterStartDate(DateTime time)
        {
            var startTime = $"{time.Month}/{time.Day}/{time.Year} {time.Hour}:{time.Minute}:{time.Second}";
            var query =
$@"SELECT DocNo FROM CCDocument
WHERE DocCategory = 'Benefits/Out-Going'
AND DirectionFg = 'O'
AND DocSource = 'O'
AND DocDate > {startTime}
ORDER BY DocDate DESC;
";
             return _database.Query<W2BatchPrintRow>(query).Select(row => new DocumentDetails{
                 DocumentId = row.DocumentNumber,
                 DocumentCreator = row.CreatedBy
                     }).ToList();
        }

        public class W2BatchPrintRow
        {
            public string DocumentNumber { get; set; }
            public string CreatedBy { get; set; }
        }
    }
}
