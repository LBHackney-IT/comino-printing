using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Amazon.Lambda.Core;
using AwsDotnetCsharp.UsecaseInterfaces;
using dotenv.net;
using Microsoft.Extensions.DependencyInjection;
using Gateways;
using Microsoft.Extensions.Configuration;
using UseCases;
using UseCases.GatewayInterfaces;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handlers
    {
        private readonly ServiceProvider _serviceProvider;

        public Handlers()
        {
//            DotNetEnv.Env.Load("./.env");
//            DotEnv.Config(
//                new DotEnvOptions
//                {
//                    ThrowOnError = false,
//                    EnvFile = ".env"
//                }
//            );
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .Build();

        }

        private void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IGetDocumentsIds, GetDocumentsIds>();
            serviceCollection.AddScoped<ICominoGateway, CominoGateway>();
            var cominoConnectionString = Environment.GetEnvironmentVariable("COMINO_DB_CONN_STR");
            
            Console.WriteLine("*************************");
            Console.WriteLine(cominoConnectionString);

            serviceCollection.AddTransient<IDbConnection>(sp => new SqlConnection(""));
        }

        public void FetchAndQueueDocumentIds(ILambdaContext context)
        {
            var getDocumentsUseCse = _serviceProvider.GetService<IGetDocumentsIds>();
            Console.WriteLine("hi");
//           var lambdaOutput = getDocumentsUseCse.Execute();
//           LambdaLogger.Log("Document ids retrieved" + JsonConvert.SerializeObject(lambdaOutput));
        }
    }
}
