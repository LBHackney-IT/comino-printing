using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
<<<<<<< HEAD
using System.Linq;
=======
using System.Threading.Tasks;
>>>>>>> Get html from documents gateway
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using AwsDotnetCsharp.UsecaseInterfaces;
using Gateways;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using UseCases;
using UseCases.GatewayInterfaces;
using Usecases.UseCaseInterfaces;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handlers
    {
        private ServiceProvider _serviceProvider;

        public Handlers() {
            var configuration = BuildConfiguration();
            ConfigureServices(configuration);
        }

        public void FetchAndQueueDocumentIds(ILambdaContext context)
        {
            var getDocumentsUseCse = _serviceProvider.GetService<IGetDocumentsIds>();
           var lambdaOutput = getDocumentsUseCse.Execute();
           LambdaLogger.Log("Document ids retrieved" + JsonConvert.SerializeObject(lambdaOutput));

           if (!lambdaOutput.Any()) return;
           var documentIds = lambdaOutput.Select(documentDetail => documentDetail.DocumentId).ToList();
           var pushIdsToSqsUseCase = _serviceProvider.GetService<IPushIdsToSqs>();
           var sqsOutput = pushIdsToSqsUseCase.Execute(documentIds);
           LambdaLogger.Log("Response from SQS Queue:" + JsonConvert.SerializeObject(sqsOutput));
        }

        public async Task ListenForSqsEvents(SQSEvent sqsEvent, ILambdaContext context)
        {
            var listenForSqsEventsUseCase = _serviceProvider.GetService<IProcessEvents>();
            await listenForSqsEventsUseCase.Execute(sqsEvent);
        }

        private void ConfigureServices(IConfigurationRoot configurationRoot)
        {
            var services = new ServiceCollection();

            services.AddScoped<IGetDocumentsIds, GetDocumentsIds>();
            services.AddScoped<ICominoGateway, CominoGateway>();
            services.AddScoped<ISqsGateway, SqsGateway>();
            services.AddScoped<IPushIdsToSqs, PushIdsToSqs>();
            services.AddScoped<ILocalDatabaseGateway, LocalDatabaseGateway>();
            services.AddScoped<ISaveRecordsToLocalDatabase, SaveRecordsToLocalDatabase>();
            services.AddScoped<IProcessEvents, ProcessEvents>();
            services.AddScoped<IGetHtmlDocument, GetHtmlDocument>();
            services.AddScoped<IConvertHtmlToPdf, ConvertHtmlToPdf>();
            services.AddScoped<ISavePdfToS3, SavePdfToS3>();
            services.AddHttpClient<IW2DocumentsGateway, W2DocumentsGateway>();
            services.AddScoped<IGetParser, ParserLookup>();

            var cominoConnectionString = Environment.GetEnvironmentVariable("COMINO_DB_CONN_STR");
            services.AddTransient<IDbConnection>(sp => new SqlConnection(cominoConnectionString));

            services.AddTransient<IAmazonSQS>(sp => new AmazonSQSClient(RegionEndpoint.EUWest2));

            var tableName = Environment.GetEnvironmentVariable("LETTERS_TABLE_NAME");
            LambdaLogger.Log($"Dynamo table name {tableName}");
            var dynamoConfig = new AmazonDynamoDBConfig {RegionEndpoint = RegionEndpoint.EUWest2};
            services.AddSingleton<IDynamoDBHandler>(sp => new DynamoDBHandler(dynamoConfig, tableName));

            _serviceProvider = services.BuildServiceProvider();
        }

        private IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
