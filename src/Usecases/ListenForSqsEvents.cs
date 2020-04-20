using Amazon.Lambda.SQSEvents;
using AwsDotnetCsharp.UsecaseInterfaces;
using System;
using System.Collections.Generic;

namespace UseCases
{
    public class ListenForSqsEvents : IListenForSqsEvents
    {
        public List<string> Execute(SQSEvent sqsEvent)
        {
            var receivedDocumentIds = new List<string>();

            // expected Records count = 1, per batchSize configured in serverless.yml
            foreach (var record in sqsEvent.Records)
            {
                var documentId = record.Body;

                // anything written to Console will be logged as CloudWatch Logs events
                Console.WriteLine($"Received from queue [{record.EventSourceArn}] documentId = {documentId}");

                receivedDocumentIds.Add(documentId);
            }


            return receivedDocumentIds;
        }
    }
}
