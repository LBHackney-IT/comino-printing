using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            var documentConfig = GetDocumentConfig();
            var categories = documentConfig.Categories;
            var descriptions = documentConfig.Descriptions;
            var startTime = $"{time.Month}/{time.Day}/{time.Year} {time.Hour}:{time.Minute}:{time.Second}";

            //TODO Remove top 5 when timespan is set to 1 minute
            var query =
                $@"SELECT TOP 5 DocNo AS DocumentNumber,
                StoreDate AS Date,
                strDescription AS LetterType,
                strUser AS UserName,
                RefType AS DocumentType
                FROM W2BatchPrint
                JOIN CCDocument on DocNo = nDocNo
                WHERE CCDocument.DocCategory IN ({String.Join(",", categories)})
                AND CCDocument.DocDesc IN ({String.Join(",", descriptions)})
                AND CCDocument.DirectionFg = 'O'
                AND CCDocument.DocSource = 'O'
                AND CCDocument.DocDate > '{startTime}'
                ORDER BY CCDocument.DocDate DESC;
                ";

            Console.WriteLine(query);
            List<DocumentDetails> queryResults;

            try
            {
                queryResults = _database.Query<W2BatchPrintRow>(query).Select(row =>
                    new DocumentDetails{
                        DocumentId = row.DocumentNumber,
                        DocumentCreator = row.UserName,
                        LetterType = row.LetterType,
                        DocumentType = row.DocumentType
                    }).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return queryResults;
        }

        public class W2BatchPrintRow
        {
            public string DocumentNumber { get; set; }
            public string UserName { get; set; }
            public string LetterType { get; set; }
            public string DocumentType { get; set; }
        }

        public class DocumentConfig
        {
            public List<string> Categories { get; set; }
            public List<string> Descriptions { get; set; }
        }

        public DocumentConfig GetDocumentConfig()
        {
            return JsonConvert.DeserializeObject<DocumentConfig>(
                Environment.GetEnvironmentVariable("DOCUMENT_CONFIG")
            );
        }
    }
}
