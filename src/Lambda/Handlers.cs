using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AwsDotnetCsharp.UsecaseInterfaces;
using Gateways;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using UseCases;
using UseCases.GatewayInterfaces;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handlers
    {
        private readonly ServiceProvider _serviceProvider;

        public Handlers() {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public void FetchAndQueueDocumentIds(ILambdaContext context)
        {
            var getDocumentsUseCse = _serviceProvider.GetService<IGetDocumentsIds>();
           var lambdaOutput = getDocumentsUseCse.Execute();
           LambdaLogger.Log("Document ids retrieved" + JsonConvert.SerializeObject(lambdaOutput));
        }

        public void PushIdsToSqs(List<string> documentIds)
        {
            var pushIdsToSqsUseCase = _serviceProvider.GetService<IPushIdsToSqs>();
            var lambdaOutput = pushIdsToSqsUseCase.Execute(documentIds);
            LambdaLogger.Log("Send Message Responses:" + JsonConvert.SerializeObject(lambdaOutput));
        }

        public void ListenForSqsEvents(SQSEvent sqsEvent)
        {
            var listenForSqsEventsUseCase = _serviceProvider.GetService<IListenForSqsEvents>();
            var lambdaOutput = listenForSqsEventsUseCase.Execute(sqsEvent);
           LambdaLogger.Log("Received from SQS: " + JsonConvert.SerializeObject(lambdaOutput));
        }

        private void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IGetDocumentsIds, GetDocumentsIds>();
            serviceCollection.AddScoped<ICominoGateway, CominoGateway>();
            serviceCollection.AddScoped<ISqsGateway, SqsGateway>();
            serviceCollection.AddScoped<IPushIdsToSqs, PushIdsToSqs>();
            var cominoConnectionString = Environment.GetEnvironmentVariable("COMINO_DB_CONN_STR");
            LambdaLogger.Log($"Fetched Connection string: {cominoConnectionString != null}");
            LambdaLogger.Log($"Stage env: {Environment.GetEnvironmentVariable("ENV")}");
            serviceCollection.AddTransient<IDbConnection>(sp => new SqlConnection(cominoConnectionString));
        }
    }
}
