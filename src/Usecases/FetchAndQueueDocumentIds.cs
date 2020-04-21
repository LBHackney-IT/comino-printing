using System.Linq;
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

        public FetchAndQueueDocumentIds(IGetDocumentsIds getDocumentIds, IPushIdsToSqs pushIdsToSqs)
        {
            _getDocumentIds = getDocumentIds;
            _pushIdsToSqs = pushIdsToSqs;
        }

        public void Execute()
        {
            var lambdaOutput = _getDocumentIds.Execute();
            LambdaLogger.Log("Document ids retrieved" + JsonConvert.SerializeObject(lambdaOutput));

            if (!lambdaOutput.Any()) return;
            var documentIds = lambdaOutput.Select(documentDetail => documentDetail.DocumentId).ToList();
            var sqsOutput = _pushIdsToSqs.Execute(documentIds);
            LambdaLogger.Log("Response from SQS Queue:" + JsonConvert.SerializeObject(sqsOutput));
        }
    }
}