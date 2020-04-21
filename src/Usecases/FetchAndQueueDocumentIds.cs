using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using AwsDotnetCsharp.UsecaseInterfaces;
using Newtonsoft.Json;
using Usecases.UseCaseInterfaces;

namespace UseCases
{
    public class FetchAndQueueDocumentIds : IFetchAndQueueDocumentIds
    {
        private readonly IGetDocumentsIds _getDocumentIds;
        private readonly IPushIdsToSqs _pushIdsToSqs;
        private readonly ISaveRecordsToLocalDatabase _saveRecordsToLocalDatabase;

        public FetchAndQueueDocumentIds(IGetDocumentsIds getDocumentIds, IPushIdsToSqs pushIdsToSqs,
            ISaveRecordsToLocalDatabase saveRecordsToLocalDatabase)
        {
            _getDocumentIds = getDocumentIds;
            _pushIdsToSqs = pushIdsToSqs;
            _saveRecordsToLocalDatabase = saveRecordsToLocalDatabase;
        }

        public async Task Execute()
        {
            var documentsToProcess = _getDocumentIds.Execute();
            LambdaLogger.Log("Document ids retrieved" + JsonConvert.SerializeObject(documentsToProcess));

            if (!documentsToProcess.Any()) { return; }

            var docsWithTimestamps = await _saveRecordsToLocalDatabase.Execute(documentsToProcess);
            LambdaLogger.Log("Document details added to Dynamo DB");
            var documentIds = docsWithTimestamps.Select(documentDetail => documentDetail.SavedAt).ToList();
            var sqsOutput = _pushIdsToSqs.Execute(documentIds);
            LambdaLogger.Log("Response from SQS Queue:" + JsonConvert.SerializeObject(sqsOutput));
        }
    }
}