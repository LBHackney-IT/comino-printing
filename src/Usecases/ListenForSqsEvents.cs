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
            var processedDocumentIds = new List<string>();

            // the number of records should be 1, as per batchSize configuration 
            // in ./src/Lambda/serverless.yml
            foreach (var record in sqsEvent.Records)
            {
                var docNo = record.Body;

                // anything written to Console will be logged as CloudWatch Logs events
                Console.WriteLine($"Received from queue [{record.EventSourceArn}] DocNo = {docNo}");

                processedDocumentIds.Add(docNo);
            }


            return processedDocumentIds;
        }
    }
}
