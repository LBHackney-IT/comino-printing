using Amazon.Lambda.SQSEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boundary.UseCaseInterfaces;
using Newtonsoft.Json;
using Usecases.Domain;
using Usecases.Enums;
using Usecases.GatewayInterfaces;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;
using Usecases;

namespace UseCases
{
    public class ProcessEvents : IProcessEvents
    {
        private readonly IGetHtmlDocument _getHtmlDocument;
        private readonly IConvertHtmlToPdf _convertHtmlToPdf;
        private readonly IS3Gateway _savePdfToS3;
        private readonly IGetDetailsOfDocumentForProcessing _getDocumentDetails;
        private readonly IDbLogger _logger;
        private readonly ILocalDatabaseGateway _localDatabaseGateway;

        public ProcessEvents(IGetHtmlDocument getHtmlDocument, IConvertHtmlToPdf convertHtmlToPdf,
            IS3Gateway savePdfToS3, IGetDetailsOfDocumentForProcessing getDocumentDetails, IDbLogger logger,
            ILocalDatabaseGateway localDatabaseGateway)
        {
            _getHtmlDocument = getHtmlDocument;
            _convertHtmlToPdf = convertHtmlToPdf;
            _savePdfToS3 = savePdfToS3;
            _getDocumentDetails = getDocumentDetails;
            _logger = logger;
            _localDatabaseGateway = localDatabaseGateway;
        }

        public async Task Execute(SQSEvent sqsEvent)
        {
            Console.WriteLine("Received message from SQS");
            // expected Records count = 1, per batchSize configured in serverless.yml
            // Messages will be removed from the queue upon successful response of this lambda.
            // If no successful response within 30sec then they will be available to pick up from the queue again.

            Console.WriteLine("Getting document configuration");
            var documentConfig = ParsingHelpers.GetDocumentConfig();
            var automaticApprovals = documentConfig.AutomaticApprovals;

            var record = sqsEvent.Records.First();
            var timestamp = record.Body;

            Console.WriteLine($"Received from queue [{record.EventSourceArn}] document timestamp = {timestamp}");

            var document = await _getDocumentDetails.Execute(timestamp);
            Console.WriteLine($"Received Document {JsonConvert.SerializeObject(document)}");

            if (document == null)
            {
                Console.WriteLine($"Could not find document for ID {timestamp} waiting to be processed in Dynamo");
                return;
            }

            await _logger.LogMessage(timestamp, "Picked up document from queue - Processing");
            Console.WriteLine($"Retrieved from dynamo, getting Html for documentId = {document.CominoDocumentNumber}");

            var html = await TryGetDocumentAsHtml(document, timestamp);
            Console.WriteLine($"Received HTML: {(html == null ? "" : (html.Length < 100 ? html : html.Substring(0, 100)))}");

            await TryConvertToPdf(html, document, timestamp);
            await TryStoreInS3(document, timestamp);

            if (automaticApprovals != null && automaticApprovals.Contains(document.LetterType))
            {
                 await _localDatabaseGateway.UpdateStatus(document.Id, LetterStatusEnum.ReadyForGovNotify);
            }
            else
            {
                await _localDatabaseGateway.UpdateStatus(document.Id, LetterStatusEnum.WaitingForApproval);
            }
        }

        private async Task TryStoreInS3(DocumentDetails document, string timestamp)
        {
            try
            {
                var result = await _savePdfToS3.SavePdfDocument(document.Id);
                Console.WriteLine($"> s3PutResult: \n{JsonConvert.SerializeObject(result)}");
            }
            catch (Exception error)
            {
                await HandleError(timestamp, error, "Failed to save to S3.");
            }

            await _logger.LogMessage(timestamp, "Stored in S3 - Ready for approval");
        }

        private async Task TryConvertToPdf(string html, DocumentDetails document, string timestamp)
        {
            try
            {
                await _convertHtmlToPdf.Execute(html, document.LetterType, document.Id);
            }
            catch (Exception error)
            {
                await HandleError(timestamp, error, "Failed converting HTML to PDF.");
            }

            await _logger.LogMessage(timestamp, "Converted To Pdf");
        }

        private async Task<string> TryGetDocumentAsHtml(DocumentDetails document, string timestamp)
        {
            var html = "";
            try
            {
                html = await _getHtmlDocument.Execute(document.CominoDocumentNumber);
            }
            catch (Exception error)
            {
                await HandleError(timestamp, error, "Failed getting HTML from Documents API.");
            }

            await _logger.LogMessage(timestamp, "Retrieved Html from Documents API");
            return html;
        }

        private async Task HandleError(string timestamp, Exception error, string message)
        {
            await _localDatabaseGateway.UpdateStatus(timestamp, LetterStatusEnum.ProcessingError);
            await _logger.LogMessage(timestamp, $"{message} Error message: {error.Message}");
            Console.WriteLine($"error {error}");
            throw error;
        }
    }
}
