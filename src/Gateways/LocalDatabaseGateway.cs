using System;
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
            _databaseClient = database.DynamoDBClient;
        }

        public async Task<string> SaveDocument(DocumentDetails newDocument)
        {
            var currentTimestamp = CurrentUtcUnixTimestamp();
            while (true)
            {
                var documentItem = ConstructDynamoDocument(newDocument, currentTimestamp);

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

        public async Task<List<DocumentDetails>> GetAllRecords(int limit, string cursor)
        {
            var scanFilter = new ScanFilter();

            var search = _documentsTable.Scan(scanFilter);
            var records = await search.GetRemainingAsync();

            var parsedRecords = ParseRecords(records);

            return parsedRecords.OrderByDescending(doc => DateTime.Parse(doc.Id))
                .Where(doc => cursor == null || DateTime.Parse(doc.Id) < DateTime.Parse(cursor))
                .Take(limit).ToList();
        }

        public async Task<DocumentDetails> GetRecordByTimeStamp(string id)
        {
            var config = new GetItemOperationConfig{ ConsistentRead = true };
            var document = await _documentsTable.GetItemAsync(id, config);
            return ParseRecord(document);
        }

        public async Task<DocumentDetails> RetrieveDocumentAndSetStatusToProcessing(string id)
        {
            var updateDoc = new Document
            {
                ["InitialTimestamp"] = id,
                ["Status"] = LetterStatusEnum.Processing.ToString(),
            };
            var response = await _documentsTable.UpdateItemAsync(updateDoc, new UpdateItemOperationConfig{ReturnValues = ReturnValues.AllOldAttributes});
            if (response.Count == 0 || response["Status"] == LetterStatusEnum.Processing.ToString() )
            {
                return null;
            }
            return ParseRecord(response);
        }

        public async Task<List<DocumentDetails>> GetDocumentsThatAreReadyForGovNotify()
        {
            var records = await GetLettersWithStatus(LetterStatusEnum.ReadyForGovNotify);

            Console.WriteLine("> records[0].Status:");
            Console.WriteLine(records[0].Status);

            return records.ToList();
        }

        public async Task<UpdateStatusResponse> UpdateStatus(string id, LetterStatusEnum newStatus)
        {
            var updateDoc = new Document
            {
                ["InitialTimestamp"] = id,
                ["Status"] = newStatus.ToString(),
            };
            await _documentsTable.UpdateItemAsync(updateDoc, new UpdateItemOperationConfig{ReturnValues = ReturnValues.AllNewAttributes});
            return new UpdateStatusResponse();
        }

        public async Task LogMessage(string id, string message)
        {
            var timestamp = CurrentUtcUnixTimestamp();
            var update = UpdateRequestToAppendLogMessage(id, message, timestamp);

            try
            {
                await _databaseClient.UpdateItemAsync(update);
            }
            catch (ConditionalCheckFailedException)
            {
                var createLog = UpdateRequestToCreateLogWithMessage(id, message, timestamp);
                try
                {
                    await _databaseClient.UpdateItemAsync(createLog);
                }
                catch (ConditionalCheckFailedException)
                {
                    Console.WriteLine($"Cant write log to document id {id}, item doesnt exist in database");
                }
            }
        }

        public DocumentLog GetLogForDocument(string id)
        {
            var log = _documentsTable.GetItemAsync(id).Result["Log"];
            var logEntries = new Dictionary<string, string>();
            log.AsDocument().ToList().ForEach( x => logEntries[x.Key] = x.Value.ToString());
            return new DocumentLog{Entries = logEntries};
        }

        public async Task<List<DocumentDetails>> GetLettersWaitingForGovNotify()
        {
            var sentToGovNotifyStatus = await GetLettersWithStatus(LetterStatusEnum.SentToGovNotify);
            var pendingVirusCheckStatus = await GetLettersWithStatus(LetterStatusEnum.GovNotifyPendingVirusCheck);

            return sentToGovNotifyStatus.Concat(pendingVirusCheckStatus).ToList();
        }

        private List<DocumentDetails> ParseRecords(List<Document> records)
        {
            return records.Select(ParseRecord).ToList();
        }

        private static DocumentDetails ParseRecord(Document document)
        {
            var logEntries = new Dictionary<string, string>();
            if (document.ContainsKey("Log"))
            {
                document["Log"].AsDocument().ToList().ForEach(x => logEntries[x.Key] = x.Value.ToString());
            }

            return new DocumentDetails
            {
                DocumentCreator = document["DocumentCreatorUserName"],
                CominoDocumentNumber = document["CominoDocumentNumber"],
                DocumentType = document["DocumentType"],
                LetterType = document["LetterType"],
                Id = document["InitialTimestamp"],
                Status = Enum.Parse<LetterStatusEnum>(document["Status"]),
                Log = logEntries
            };
        }

        private async Task<List<DocumentDetails>> GetLettersWithStatus(LetterStatusEnum status)
        {
            var tableName = _documentsTable.TableName;
            var queryRequest = new QueryRequest
            {
                TableName = tableName,
                IndexName = "Status",
                ScanIndexForward = true,
                KeyConditionExpression = "#status = :value",
                ExpressionAttributeNames = new Dictionary<string, string> {{"#status", "Status"}},
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":value", new AttributeValue {S = status.ToString()}},
                },
            };
            var results = await _databaseClient.QueryAsync(queryRequest);
            return results.Items.Select(entry => new DocumentDetails
            {
                DocumentCreator = entry["DocumentCreatorUserName"]?.S?.ToString(),
                CominoDocumentNumber = entry["CominoDocumentNumber"]?.S?.ToString(),
                DocumentType = entry["DocumentType"]?.S?.ToString(),
                LetterType = entry["LetterType"]?.S?.ToString(),
                Id = entry["InitialTimestamp"]?.S?.ToString(),
                Status = Enum.Parse<LetterStatusEnum>(entry["Status"]?.S?.ToString()),
            }).ToList();
        }
        private static Document ConstructDynamoDocument(DocumentDetails newDocument, string currentTimestamp)
        {
            return new Document
            {
                ["CominoDocumentNumber"] = newDocument.CominoDocumentNumber,
                ["DocumentCreatorUserName"] = newDocument.DocumentCreator,
                ["LetterType"] = newDocument.LetterType,
                ["DocumentType"] = newDocument.DocumentType,
                ["InitialTimestamp"] = currentTimestamp,
                ["Status"] = "Waiting",
            };
        }

        private static PutItemOperationConfig ConditionalOnTimestampUniqueness(string id)
        {
            return new PutItemOperationConfig
            {
                ConditionalExpression = new Expression
                {
                    ExpressionStatement = "InitialTimestamp <> :t",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>{
                    {
                        ":t", id
                    }}
                },
            };
        }

        private static string CurrentUtcUnixTimestamp()
        {
            return DateTime.UtcNow.ToString("O");
        }

        private UpdateItemRequest UpdateRequestToCreateLogWithMessage(string id, string message, string timestamp)
        {
            var log = new AttributeValue
                {M = new Dictionary<string, AttributeValue> {{timestamp, new AttributeValue (message)}}};
            return new UpdateItemRequest
            {
                TableName = _documentsTable.TableName,
                UpdateExpression = "SET #atr = :val",
                Key = new Dictionary<string, AttributeValue> {{"InitialTimestamp", new AttributeValue {S = id}}},
                ExpressionAttributeNames = new Dictionary<string, string> {{"#atr", "Log"}},
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {{":val", log}},
                ConditionExpression = "attribute_exists(InitialTimestamp)"
            };
        }

        private UpdateItemRequest UpdateRequestToAppendLogMessage(string id, string message, string timestamp)
        {
            var newLogEntry = new AttributeValue (message);
            return new UpdateItemRequest
            {
                TableName = _documentsTable.TableName,
                UpdateExpression = "SET #atr.#timestamp = :val",
                Key = new Dictionary<string, AttributeValue> {{"InitialTimestamp", new AttributeValue {S = id}}},
                ExpressionAttributeNames = new Dictionary<string, string> {{"#atr", "Log"}, {"#timestamp", timestamp}},
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {{":val", newLogEntry}},
                ConditionExpression = "attribute_exists(#atr)"
            };
        }
    }
}