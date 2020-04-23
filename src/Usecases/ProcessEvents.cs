using Amazon.Lambda.SQSEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using Newtonsoft.Json;
using Usecases.Domain;
using Usecases.GatewayInterfaces;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

namespace UseCases
{
    public class ProcessEvents : IProcessEvents
    {
        private readonly IGetHtmlDocument _getHtmlDocument;
        private readonly IConvertHtmlToPdf _convertHtmlToPdf;
        private readonly IS3Gateway _savePdfToS3;
        private IGetDetailsOfDocumentForProcessing _getDocumentDetails;
        private IDbLogger _logger;

        public ProcessEvents(IGetHtmlDocument getHtmlDocument, IConvertHtmlToPdf convertHtmlToPdf,
            IS3Gateway savePdfToS3, IGetDetailsOfDocumentForProcessing getDocumentDetails, IDbLogger logger)
        {
            _getHtmlDocument = getHtmlDocument;
            _convertHtmlToPdf = convertHtmlToPdf;
            _savePdfToS3 = savePdfToS3;
            _getDocumentDetails = getDocumentDetails;
            _logger = logger;
        }

        public async Task Execute(SQSEvent sqsEvent)
        {
            Console.WriteLine("Received message from SQS");
            // expected Records count = 1, per batchSize configured in serverless.yml
            var record = sqsEvent.Records.First();
            //TODO Remove message from the queue
            var timestamp = record.Body;

            Console.WriteLine($"Received from queue [{record.EventSourceArn}] document timestamp = {timestamp}");

            //TODO Check the status is set to waiting
            var document = await _getDocumentDetails.Execute(timestamp);
            Console.WriteLine($"Received Document {JsonConvert.SerializeObject(document)}");

            if (document == null)
            {
                Console.WriteLine($"Could not find document for ID {timestamp} waiting to be processed in Dynamo");
                return;
            }

            await _logger.LogMessage(timestamp, "Picked up document from queue - Processing");
            Console.WriteLine($"Retrieved from dynamo, getting Html for documentId = {document.DocumentId}");

            var html = await TryGetDocumentAsHtml(document, timestamp);
            Console.WriteLine($"> htmlDoc:\n{html}");

            await TryConvertToPdf(html, document, timestamp);
            await TryStoreInS3(document, timestamp);
        }

        private async Task TryStoreInS3(DocumentDetails document, string timestamp)
        {
            try
            {
                var result = _savePdfToS3.SavePdfDocument(document.DocumentId);
                Console.WriteLine($"> s3PutResult:\n{result}");
            }
            catch (Exception error)
            {
                await _logger.LogMessage(timestamp, $"Failed to save to S3. Error message: {error.Message}");
                throw;
            }

            await _logger.LogMessage(timestamp, "Stored in S3 - Ready for approval");
        }

        private async Task TryConvertToPdf(string html, DocumentDetails document, string timestamp)
        {
            try
            {
                _convertHtmlToPdf.Execute(html, document.LetterType, document.DocumentId);
            }
            catch (Exception error)
            {
                await _logger.LogMessage(timestamp, $"Failed converting HTML to PDF. Error message: {error.Message}");
                throw;
            }

            await _logger.LogMessage(timestamp, "Converted To Pdf");
        }

        private async Task<string> TryGetDocumentAsHtml(DocumentDetails document, string timestamp)
        {
            string html;
            try
            {
                html = await _getHtmlDocument.Execute(document.DocumentId);
            }
            catch (Exception error)
            {
                await _logger.LogMessage(timestamp, $"Failed getting HTML from Documents API. Error message: {error.Message}");
                throw;
            }

            await _logger.LogMessage(timestamp, "Retrieved Html from Documents API");
            return html;
        }
    }
}
