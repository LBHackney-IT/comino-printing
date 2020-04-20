using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AwsDotnetCsharp.UsecaseInterfaces;
using Gateways;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using UseCases;
using UseCases.GatewayInterfaces;

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
            var cominoConnectionString = Environment.GetEnvironmentVariable("COMINO_DB_CONN_STR");
            LambdaLogger.Log($"Fetched Connection string: {cominoConnectionString != null}");
            LambdaLogger.Log($"Stage variable {Environment.GetEnvironmentVariable("ENV")}");
            serviceCollection.AddTransient<IDbConnection>(sp => new SqlConnection(cominoConnectionString));

            var dynamoConfig = new AmazonDynamoDBConfig {RegionEndpoint = RegionEndpoint.EUWest2};
            serviceCollection.AddSingleton<IDynamoDBHandler>(sp => new DynamoDBHandler(dynamoConfig));

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
