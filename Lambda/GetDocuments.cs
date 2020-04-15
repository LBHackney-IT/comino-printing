using Amazon.Lambda.Core;
using System;
using System.Threading;
using AwsDotnetCsharp.UsecaseInterfaces;
using Newtonsoft.Json;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class GetDocuments
    {
        private readonly IGetDocumentsIds _getDocumentsIds;

        public GetDocuments(IGetDocumentsIds getDocumentsIds)
        {
            _getDocumentsIds = getDocumentsIds;
        }

        public void FetchDocumentIds(ILambdaContext context)
        {
           var lambdaOutput = _getDocumentsIds.Execute();
           LambdaLogger.Log("Document ids retrieved" + JsonConvert.SerializeObject(lambdaOutput));
        }
    }
}
