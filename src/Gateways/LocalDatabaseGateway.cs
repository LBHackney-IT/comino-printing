using System;
using System.Collections.Generic;
using System.Threading;
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

        public async Task<DocumentDetails> GetRecordByTimeStamp(string currentTimestamp)
        {
            var config = new GetItemOperationConfig{ ConsistentRead = true };
            var document = await _documentsTable.GetItemAsync(currentTimestamp, config);
            return new DocumentDetails
            {
                DocumentCreator = document["DocumentCreatorUserName"],
                DocumentId = document["DocumentId"],
                DocumentType = document["DocumentType"],
                LetterType = document["LetterType"],
                SavedAt = document["InitialTimestamp"],
            };
        }

        private static Document ConstructDocument(DocumentDetails newDocument, string currentTimestamp)
        {
            return new Document
            {
                ["DocumentId"] = newDocument.DocumentId,
                ["DocumentCreatorUserName"] = newDocument.DocumentCreator,
                ["LetterType"] = newDocument.LetterType,
                ["DocumentType"] = newDocument.DocumentType,
                ["InitialTimestamp"] = currentTimestamp,
                ["Status"] = "Waiting",
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