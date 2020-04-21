using Amazon.Lambda.SQSEvents;
using AwsDotnetCsharp.UsecaseInterfaces;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Usecases.UseCaseInterfaces;

namespace UseCases
{
    public class ProcessEvents : IProcessEvents
    {
        private readonly IGetHtmlDocument _getHtmlDocument;
        private readonly IConvertHtmlToPdf _convertHtmlToPdf;
        private readonly ISavePdfToS3 _savePdfToS3;

        public ProcessEvents(IGetHtmlDocument getHtmlDocument, IConvertHtmlToPdf convertHtmlToPdf,
            ISavePdfToS3 savePdfToS3)
        {
            _getHtmlDocument = getHtmlDocument;
            _convertHtmlToPdf = convertHtmlToPdf;
            _savePdfToS3 = savePdfToS3;
        }

        public void Execute(SQSEvent sqsEvent)
        {
            Console.Write("Received from SQS: " + JsonConvert.SerializeObject(sqsEvent));
            // expected Records count = 1, per batchSize configured in serverless.yml
            foreach (var record in sqsEvent.Records)
            {
                //TODO: ADD logging to local database 
                var documentId = record.Body;

                // anything written to Console will be logged as CloudWatch Logs events
                Console.WriteLine($"Received from queue [{record.EventSourceArn}] documentId = {documentId}");
                Console.WriteLine($"Getting Html for documentId = {documentId}");

                var html = _getHtmlDocument.Execute(documentId);

                Console.WriteLine($"> htmlDoc:\n{html}");
                var pdfBytes = _convertHtmlToPdf.Execute(html);

                var result = _savePdfToS3.Execute(documentId, pdfBytes);
                Console.WriteLine($"> s3PutResult:\n{result}");
            }
        }
    }
}
