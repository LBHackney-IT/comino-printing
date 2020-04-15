using Amazon.Lambda.Core;
using System;
using System.Threading;
using AwsDotnetCsharp.UsecaseInterfaces;
using dotenv.net;
using dotenv.net.DependencyInjection.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class GetDocuments
    {
        private readonly IGetDocumentsIds _getDocumentsIds;

        public GetDocuments(IGetDocumentsIds getDocumentsIds)
        {
            _getDocumentsIds = getDocumentsIds;
            ReadEnvironmentVariablesFromDotEnv();
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Configuration = serviceProvider.GetService<ILambdaConfiguration>();

        }

        private void ConfigureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IGetDocumentsIds, GetDocumentsIds>();
        }

        public void FetchDocumentIds(ILambdaContext context)
        {
           var lambdaOutput = _getDocumentsIds.Execute();
           LambdaLogger.Log("Document ids retrieved" + JsonConvert.SerializeObject(lambdaOutput));
        }

        private static void ReadEnvironmentVariablesFromDotEnv()
        {
            DotEnv.Config(
                new DotEnvOptions
                {
                    ThrowOnError = false,
                    EnvFile = ".env"
                }
            );
        }
    }
}
