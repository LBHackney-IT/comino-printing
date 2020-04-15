using System;
using System.Collections.Generic;
using System.Linq;
using UseCases.GatewayInterfaces;

namespace Gateways
{
    public class CominoGateway : ICominoGateway
    {
        private IDatabaseRepository _database;

        public CominoGateway(IDatabaseRepository database)
        {
            _database = database;
        }

        public List<string> GetDocumentsAfterStartDate(DateTime time)
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
             return _database.QueryBatchPrint(query).Select(row => row.DocumentNumber).ToList();
        }
    }
}
