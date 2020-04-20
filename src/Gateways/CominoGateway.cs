﻿using System;
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
$@"SELECT DocNo AS DocumentNumber,
StoreDate AS Date,
strDescription AS LetterType,
strUser AS UserName,
RefType AS DocumentType
FROM W2BatchPrint
JOIN CCDocument on DocNo = nDocNo
WHERE CCDocument.DocCategory = 'Benefits/Out-Going'
AND CCDocument.DirectionFg = 'O'
AND CCDocument.DocSource = 'O'
AND CCDocument.DocDate > {startTime}
ORDER BY CCDocument.DocDate DESC;
";
             return _database.Query<W2BatchPrintRow>(query).Select(row =>
                 new DocumentDetails{
                     DocumentId = row.DocumentNumber,
                     DocumentCreator = row.UserName,
                     LetterType = row.LetterType,
                     DocumentType = row.DocumentType
            }).ToList();
        }

        public class W2BatchPrintRow
        {
            public string DocumentNumber { get; set; }
            public string UserName { get; set; }
            public string LetterType { get; set; }
            public string DocumentType { get; set; }
        }
    }
}
