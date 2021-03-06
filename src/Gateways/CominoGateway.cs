﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Usecases;
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
            var documentConfig = ParsingHelpers.GetDocumentConfig();
            var categories = documentConfig.Categories;
            var descriptions = documentConfig.Descriptions;
            var startTime = $"{time.Month}/{time.Day}/{time.Year} {time.Hour}:{time.Minute}:{time.Second}";

            var query =
                $@"SELECT DocNo AS DocumentNumber,
                StoreDate AS Date,
                strDescription AS LetterType,
                strUser AS UserName,
                RefType AS DocumentType
                FROM W2BatchPrint
                JOIN CCDocument on DocNo = nDocNo
                WHERE CCDocument.DocCategory IN ('{string.Join("','", categories)}')
                AND CCDocument.DocDesc IN ('{string.Join("','", descriptions)}')
                AND CCDocument.DirectionFg = 'O'
                AND CCDocument.DocSource = 'O'
                AND W2BatchPrint.StoreDate > '{startTime}'
                ORDER BY W2BatchPrint.StoreDate DESC;
                ";

            List<DocumentDetails> queryResults;

            try
            {
                queryResults = _database.Query<W2BatchPrintRow>(query).Select(row =>
                    new DocumentDetails {
                        Id = row.Date.ToString("O"),
                        CominoDocumentNumber = row.DocumentNumber,
                        DocumentCreator = row.UserName,
                        LetterType = row.LetterType,
                        DocumentType = row.DocumentType,
                        Date = row.Date.ToString("O")
                    }).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return queryResults;
        }

        public void MarkDocumentAsSent(string documentNumber)
        {
            var timeNow = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.0");

            var updateLastPrintedDate = $@"UPDATE CCDocument SET LastPrinted = '{timeNow}' WHERE DocNo = '{documentNumber}';";
            var deleteFromBatchPrint = $@"DELETE FROM W2BatchPrint WHERE nDocNo = '{documentNumber}'";

            _database.Query(updateLastPrintedDate);
            _database.Query(deleteFromBatchPrint);

            Console.WriteLine($"Comino DB update {documentNumber} removed from batch print and LastPrintedDate updated to {timeNow}");
        }

        public CominoSentStatusCheck GetDocumentSentStatus(string cominoDocumentNumber)
        {
            var getStatus = $@"
SELECT TOP 1 LastPrinted, nDocNo
FROM CCDocument
LEFT JOIN W2BatchPrint ON nDocNo = DocNo
WHERE CCDocument.DocNo = '{cominoDocumentNumber}';
";
            var response = _database.Query<PrintStatusRow>(getStatus).FirstOrDefault();
            if (response?.nDocNo == null || response.LastPrinted != null)
            {
                return new CominoSentStatusCheck
                {
                    Printed = true,
                    PrintedAt = response.LastPrinted
                };
            }
            return new CominoSentStatusCheck{ Printed = false };
        }

        public class W2BatchPrintRow
        {
            public string DocumentNumber { get; set; }
            public string UserName { get; set; }
            public string LetterType { get; set; }
            public string DocumentType { get; set; }
            public DateTime Date { get; set; }
        }

        public class PrintStatusRow
        {
            public string LastPrinted { get; set; }
            public string nDocNo { get; set; }
        }
    }
}
