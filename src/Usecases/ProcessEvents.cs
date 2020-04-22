using Amazon.Lambda.SQSEvents;
using AwsDotnetCsharp.UsecaseInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UseCases.GatewayInterfaces;
using Usecases.UseCaseInterfaces;

namespace UseCases
{
    public class ProcessEvents : IProcessEvents
    {
        private readonly IGetHtmlDocument _getHtmlDocument;
        private readonly IConvertHtmlToPdf _convertHtmlToPdf;
        private readonly IS3Gateway _savePdfToS3;
        private IGetDetailsOfDocumentForProcessing _getDocumentDetails;

        public ProcessEvents(IGetHtmlDocument getHtmlDocument, IConvertHtmlToPdf convertHtmlToPdf,
            IS3Gateway savePdfToS3, IGetDetailsOfDocumentForProcessing getDocumentDetails)
        {
            _getHtmlDocument = getHtmlDocument;
            _convertHtmlToPdf = convertHtmlToPdf;
            _savePdfToS3 = savePdfToS3;
            _getDocumentDetails = getDocumentDetails;
        }

        public async Task Execute(SQSEvent sqsEvent)
        {
            Console.WriteLine("Received message from SQS");
            // expected Records count = 1, per batchSize configured in serverless.yml
            var record = sqsEvent.Records.First();

            //TODO: ADD logging to local database

            var timestamp = record.Body;

            // anything written to Console will be logged as CloudWatch Logs events
            Console.WriteLine($"Received from queue [{record.EventSourceArn}] document timestamp = {timestamp}");

            var document = await _getDocumentDetails.Execute(timestamp);

            Console.WriteLine($"Retrieved from dynamo, getting Html for documentId = {document.DocumentId}");

            var html = await _getHtmlDocument.Execute(document.DocumentId);

            Console.WriteLine($"> htmlDoc:\n{html}");
            _convertHtmlToPdf.Execute(html, document.LetterType, document.DocumentId);

            var result = _savePdfToS3.SavePdfDocument(document.DocumentId);
            Console.WriteLine($"> s3PutResult:\n{result}");
        }
    }
}
