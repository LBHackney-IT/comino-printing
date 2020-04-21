using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
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

        public void ListenForSqsEvents(SQSEvent sqsEvent)
        {
            var listenForSqsEventsUseCase = _serviceProvider.GetService<IListenForSqsEvents>();
            var lambdaOutput = listenForSqsEventsUseCase.Execute(sqsEvent);
           LambdaLogger.Log("Received from SQS: " + JsonConvert.SerializeObject(lambdaOutput));
        }

        private void ConfigureServices(IConfigurationRoot configurationRoot)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IGetDocumentsIds, GetDocumentsIds>();
            serviceCollection.AddScoped<ICominoGateway, CominoGateway>();
            serviceCollection.AddScoped<ISqsGateway, SqsGateway>();
            serviceCollection.AddScoped<IPushIdsToSqs, PushIdsToSqs>();
            serviceCollection.AddScoped<ILocalDatabaseGateway, LocalDatabaseGateway>();
            serviceCollection.AddScoped<ISaveRecordsToLocalDatabase, SaveRecordsToLocalDatabase>();

            var cominoConnectionString = Environment.GetEnvironmentVariable("COMINO_DB_CONN_STR");
            serviceCollection.AddTransient<IDbConnection>(sp => new SqlConnection(cominoConnectionString));

            serviceCollection.AddTransient<IAmazonSQS>(sp => new AmazonSQSClient(RegionEndpoint.EUWest2));

            var tableName = Environment.GetEnvironmentVariable("LETTERS_TABLE_NAME");
            LambdaLogger.Log($"Dynamo table name {tableName}");
            var dynamoConfig = new AmazonDynamoDBConfig {RegionEndpoint = RegionEndpoint.EUWest2};
            serviceCollection.AddSingleton<IDynamoDBHandler>(sp => new DynamoDBHandler(dynamoConfig, tableName));

            _serviceProvider = serviceCollection.BuildServiceProvider();
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
