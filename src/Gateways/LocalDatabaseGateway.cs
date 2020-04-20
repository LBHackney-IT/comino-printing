using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Usecases.Domain;
using UseCases.GatewayInterfaces;

namespace Gateways
{
    public class LocalDatabaseGateway : ILocalDatabaseGateway
    {
        private readonly Table _documentsTable;

        public LocalDatabaseGateway(IDynamoDBHandler database)
        {
            _documentsTable = database.DocumentTable;
        }

        public async Task<string> SaveDocument(DocumentDetails newDocument)
        {
            var currentTimestamp = CurrentUtcUnixTimestamp();
            while (true)
            {
                var documentItem = ConstructDocument(newDocument, currentTimestamp);

                try
                {
                    var putConfig = ConditionalOnTimestampUniqueness(currentTimestamp);
                    await _documentsTable.PutItemAsync(documentItem, putConfig);
                }
                catch (ConditionalCheckFailedException)
                {
                    currentTimestamp = CurrentUtcUnixTimestamp();
                    continue;
                }

                break;
            }

            return currentTimestamp;
        }

        private static Document ConstructDocument(DocumentDetails newDocument, string currentTimestamp)
        {
            return new Document
            {
                ["DocumentId"] = newDocument.DocumentId,
                ["DocumentCreator"] = newDocument.DocumentCreator,
                ["InitialTimestamp"] = currentTimestamp,
                ["Status"] = "Waiting"
            };
        }

        private static PutItemOperationConfig ConditionalOnTimestampUniqueness(string currentTimestamp)
        {
            return new PutItemOperationConfig
            {
                ConditionalExpression = new Expression
                {
                    ExpressionStatement = "InitialTimestamp <> :t",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>{
                    {
                        ":t", currentTimestamp
                    }}
                },
            };
        }

        private static string CurrentUtcUnixTimestamp()
        {
            return Convert.ToInt32((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds).ToString();
        }
    }
}