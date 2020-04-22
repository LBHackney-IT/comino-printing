using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
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
        
        // New gateway method to return all (DynamoDB scans)
        
        public async Task<DocumentDetails> RetrieveDocumentAndSetStatusToProcessing(string savedDocumentSavedAt)
        {
            var updateDoc = new Document
            {
                ["InitialTimestamp"] = savedDocumentSavedAt,
                ["Status"] = LetterStatusEnum.Processing.ToString(),
            };
            var response = await _documentsTable.UpdateItemAsync(updateDoc, new UpdateItemOperationConfig{ReturnValues = ReturnValues.AllOldAttributes});
            if (response.Count == 0 || response["Status"] == LetterStatusEnum.Processing.ToString() )
            {
                return null;
            }
            return MapToDocumentDetails(response);
        }

        public async Task UpdateStatus(string savedDocumentSavedAt, LetterStatusEnum newStatus)
        {
            var updateDoc = new Document
            {
                ["InitialTimestamp"] = savedDocumentSavedAt,
                ["Status"] = newStatus.ToString(),
            };
            await _documentsTable.UpdateItemAsync(updateDoc, new UpdateItemOperationConfig{ReturnValues = ReturnValues.AllNewAttributes});
        }

        public async Task LogMessage(string documentSavedAt, string message)
        {
            var timestamp = CurrentUtcUnixTimestamp();
            var update = UpdateRequestToAppendLogMessage(documentSavedAt, message, timestamp);

            try
            {
                await _databaseClient.UpdateItemAsync(update);
            }
            catch (ConditionalCheckFailedException)
            {
                var createLog = UpdateRequestToCreateLogWithMessage(documentSavedAt, message, timestamp);
                await _databaseClient.UpdateItemAsync(createLog);
            }
        }

        public DocumentLog GetLogForDocument(string savedDocumentSavedAt)
        {
            var log = _documentsTable.GetItemAsync(savedDocumentSavedAt).Result["Log"];
            var logEntries = new Dictionary<string, string>();
            log.AsDocument().ToList().ForEach( x => logEntries[x.Key] = x.Value.ToString());
            return new DocumentLog{Entries = logEntries};
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
            return DateTime.UtcNow.ToString("O");
        }

        private UpdateItemRequest UpdateRequestToCreateLogWithMessage(string documentSavedAt, string message, string timestamp)
        {
            var log = new AttributeValue
                {M = new Dictionary<string, AttributeValue> {{timestamp, new AttributeValue (message)}}};
            return new UpdateItemRequest
            {
                TableName = _documentsTable.TableName,
                UpdateExpression = "SET #atr = :val",
                Key = new Dictionary<string, AttributeValue> {{"InitialTimestamp", new AttributeValue {S = documentSavedAt}}},
                ExpressionAttributeNames = new Dictionary<string, string> {{"#atr", "Log"}},
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {{":val", log}},
                ConditionExpression = "attribute_exists(InitialTimestamp)"
            };
        }

        private UpdateItemRequest UpdateRequestToAppendLogMessage(string documentSavedAt, string message, string timestamp)
        {
            var newLogEntry = new AttributeValue {S = message};
            return new UpdateItemRequest
            {
                TableName = _documentsTable.TableName,
                UpdateExpression = "SET #atr.#timestamp = :val",
                Key = new Dictionary<string, AttributeValue> {{"InitialTimestamp", new AttributeValue {S = documentSavedAt}}},
                ExpressionAttributeNames = new Dictionary<string, string> {{"#atr", "Log"}, {"#timestamp", timestamp}},
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {{":val", newLogEntry}},
                ConditionExpression = "attribute_exists(#atr)"
            };
        }
    }
}