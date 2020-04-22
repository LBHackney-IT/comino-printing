using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Usecases;
using Usecases.Domain;
using Usecases.Enums;
using Usecases.GatewayInterfaces;
using UseCases.GatewayInterfaces;

namespace Gateways
{
    public class LocalDatabaseGateway : ILocalDatabaseGateway, IDbLogger
    {
        private readonly Table _documentsTable;
        private AmazonDynamoDBClient _databaseClient;

        public LocalDatabaseGateway(IDynamoDBHandler database)
        {
            _documentsTable = database.DocumentTable;
            _databaseClient = database.DatabaseClient;
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
            return MapToDocumentDetails(document);
        }

        public async Task<DocumentDetails> RetrieveDocumentAndSetStatusToProcessing(string savedDocumentSavedAt, LetterStatusEnum newStatus)
        {
            var updateDoc = new Document
            {
                ["InitialTimestamp"] = savedDocumentSavedAt,
                ["Status"] = newStatus.ToString(),
            };
            var response = await _documentsTable.UpdateItemAsync(updateDoc, new UpdateItemOperationConfig{ReturnValues = ReturnValues.AllNewAttributes});
            return MapToDocumentDetails(response);
        }

        public async Task LogMessage(string documentSavedAt, string message)
        {
            var newLogEntry = new AttributeValue
            {
                L = new List<AttributeValue>
                {
                    new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            {CurrentUtcUnixTimestamp(), new AttributeValue {S = message}}
                        }
                    }
                }
            };
            var update = new UpdateItemRequest
            {
                TableName = _documentsTable.TableName,
                UpdateExpression = "SET #atr = list_append(#atr, :val)",
                Key = new Dictionary<string, AttributeValue>{ { "InitialTimestamp", new AttributeValue{ S = documentSavedAt}}},
                ExpressionAttributeNames = new Dictionary<string, string>{ {"#atr", "Log"}},
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {{":val", newLogEntry}}
            };

            await _databaseClient.UpdateItemAsync(update);
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

        private static DocumentDetails MapToDocumentDetails(Document document)
        {
            return new DocumentDetails
            {
                DocumentCreator = document["DocumentCreatorUserName"],
                DocumentId = document["DocumentId"],
                DocumentType = document["DocumentType"],
                LetterType = document["LetterType"],
                SavedAt = document["InitialTimestamp"],
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