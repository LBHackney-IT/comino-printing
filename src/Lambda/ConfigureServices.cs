using System;
using System.Data;
using System.Data.SqlClient;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.SQS;
using Boundary.UseCaseInterfaces;
using Gateways;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Notify.Client;
using Notify.Interfaces;
using Usecases;
using UseCases;
using Usecases.GatewayInterfaces;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

namespace AwsDotnetCsharp
{
    public static class ConfigureServices
    {
        public static void Configure(this IServiceCollection services)
        {
            //Comino Database
            var cominoConnectionString = Environment.GetEnvironmentVariable("COMINO_DB_CONN_STR");
            services.AddTransient<IDbConnection>(sp => new SqlConnection(cominoConnectionString));

            //Dynamo DB
            var tableName = Environment.GetEnvironmentVariable("LETTERS_TABLE_NAME");
            LambdaLogger.Log($"Dynamo table name {tableName}");
            var dynamoConfig = new AmazonDynamoDBConfig {RegionEndpoint = RegionEndpoint.EUWest2};
            var dynamoDbClient = new DynamoDBClient(dynamoConfig);
            services.AddTransient<IDynamoDBHandler>(sp => new DynamoDBHandler(tableName, dynamoDbClient));

            //GovNotify Client
            var govNotifyApiKey = Environment.GetEnvironmentVariable("GOV_NOTIFY_API_KEY");
            services.AddTransient<INotificationClient>(sp => new NotificationClient(govNotifyApiKey));

            //SQS
            services.AddTransient<IAmazonSQS>(sp => new AmazonSQSClient(RegionEndpoint.EUWest2));

            //S3
            services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(RegionEndpoint.EUWest2));

            //Gateways
            services.AddScoped<IS3Gateway, S3Gateway>();
            services.AddScoped<ICominoGateway, CominoGateway>();
            services.AddHttpClient<IW2DocumentsGateway, W2DocumentsGateway>();
            services.AddScoped<ISqsGateway, SqsGateway>();
            services.AddScoped<ILocalDatabaseGateway, LocalDatabaseGateway>();
            services.AddScoped<IDbLogger, LocalDatabaseGateway>();
            services.AddScoped<IGovNotifyGateway, GovNotifyGateway>();

            //UseCases
            services.AddScoped<IAuthorizeApi, AuthorizeApi>();
            services.AddScoped<IGetDocumentsIds, GetDocumentsIds>();
            services.AddScoped<IPushIdsToSqs, PushIdsToSqs>();
            services.AddScoped<ISaveRecordsToLocalDatabase, SaveRecordsToLocalDatabase>();
            services.AddScoped<IProcessEvents, ProcessEvents>();
            services.AddScoped<IGetHtmlDocument, GetHtmlDocument>();
            services.AddScoped<IConvertHtmlToPdf, ConvertHtmlToPdf>();
            services.AddScoped<IGetParser, ParserLookup>();
            services.AddScoped<IConvertHtmlToPdf, ConvertHtmlToPdf>();
            services.AddScoped<IFetchAndQueueDocumentIds, FetchAndQueueDocumentIds>();
            services.AddScoped<IGetDetailsOfDocumentForProcessing, GetDetailsOfDocumentForProcessing>();
            services.AddScoped<IParseHtmlToPdf, HtmlToPdfConversionGateway>();
            services.AddScoped<IGetAllDocuments, GetAllDocuments>();
            services.AddScoped<IApproveDocument, ApproveDocument>();
            services.AddScoped<IGeneratePdfInS3Url, GeneratePdfInS3Url>();
            services.AddScoped<IGetSingleDocumentInfo, GetSingleDocumentInfo>();
            services.AddScoped<IQueryDocumentsAndSendToNotify, QueryDocumentsAndSendToNotify>();
            services.AddScoped<ICheckSendStatusOfLetters, CheckSendStatusOfLetters>();
            services.AddScoped<ICancelDocument, CancelDocument>();
        }
    }
}
