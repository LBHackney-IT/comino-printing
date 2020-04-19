using System;
using System.Globalization;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Usecases.Domain;

namespace Gateways
{
    public class LocalDatabaseGateway
    {
        private readonly Table _documentsTable;

        public LocalDatabaseGateway(IDynamoDBHandler database)
        {
            _documentsTable = database.DocumentTable;
        }

        public async Task<string> SaveDocument(DocumentDetails newDocument)
        {
            var documentItem = new Document
            {
                ["DocumentId"] = newDocument.DocumentId,
                ["DocumentCreator"] = newDocument.DocumentCreator,
                ["InitialTimestamp"] = CurrentUtcUnixTimestamp(),
                ["Status"] = "Waiting"
            };
            await _documentsTable.PutItemAsync(documentItem);

            return null;
        }

        private static string CurrentUtcUnixTimestamp()
        {
            return Convert.ToInt32((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds).ToString();
        }
    }
}