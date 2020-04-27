using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.SQS.Model;
using Boundary.UseCaseInterfaces;
using Newtonsoft.Json;
using Usecases.Domain;
using Usecases.GatewayInterfaces;
using Usecases.Interfaces;

namespace UseCases
{
    public class FetchAndQueueDocumentIds : IFetchAndQueueDocumentIds
    {
        private readonly IGetDocumentsIds _getDocumentIds;
        private readonly IPushIdsToSqs _pushIdsToSqs;
        private readonly ISaveRecordsToLocalDatabase _saveRecordsToLocalDatabase;
        private readonly IDbLogger _logger;

        public FetchAndQueueDocumentIds(IGetDocumentsIds getDocumentIds, IPushIdsToSqs pushIdsToSqs,
            ISaveRecordsToLocalDatabase saveRecordsToLocalDatabase, IDbLogger logger)
        {
            _getDocumentIds = getDocumentIds;
            _pushIdsToSqs = pushIdsToSqs;
            _saveRecordsToLocalDatabase = saveRecordsToLocalDatabase;
            _logger = logger;
        }

        public async Task Execute()
        {
            var documentsToProcess = _getDocumentIds.Execute();
            LambdaLogger.Log("Document ids retrieved" + JsonConvert.SerializeObject(documentsToProcess));

            if (!documentsToProcess.Any()) { return; }

            var docsWithTimestamps = await _saveRecordsToLocalDatabase.Execute(documentsToProcess);
            LambdaLogger.Log("Document details added to Dynamo DB");

            await AddLogMessageForEachDocument(docsWithTimestamps, "Retrieved ID from Comino and stored");

            var documentIds = docsWithTimestamps.Select(documentDetail => documentDetail.Id).ToList();
            List<SendMessageResponse> sqsOutput;
            try
            {
                sqsOutput = _pushIdsToSqs.Execute(documentIds);
            }
            catch (Exception error)
            {
                await AddLogMessageForEachDocument(docsWithTimestamps, $"Failed adding to queue. Error message {error.Message}");
                throw;
            }

            LambdaLogger.Log("Response from SQS Queue:" + JsonConvert.SerializeObject(sqsOutput));

            await AddLogMessageForEachDocument(docsWithTimestamps, "Document added to SQS queue");
        }

        private async Task AddLogMessageForEachDocument(List<DocumentDetails> docsWithTimestamps, string message)
        {
            foreach (var doc in docsWithTimestamps)
            {
                await _logger.LogMessage(doc.Id, message);
            }
        }
    }
}