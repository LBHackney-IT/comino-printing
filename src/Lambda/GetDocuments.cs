using Amazon.Lambda.Core;
using AwsDotnetCsharp.UsecaseInterfaces;
using Gateways;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using UseCases;
using UseCases.GatewayInterfaces;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class GetDocuments
    {
        private readonly IGetDocumentsIds _getDocumentsIds;

        public GetDocuments()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            serviceCollection.BuildServiceProvider();
            DotNetEnv.Env.Load("./.env");
        }

        private void ConfigureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IGetDocumentsIds, GetDocumentsIds>();
            serviceCollection.AddScoped<ICominoGateway, CominoGateway>();
            serviceCollection.AddScoped<IDatabaseRepository, DatabaseRepository>();
        }

        public void FetchDocumentIds(ILambdaContext context)
        {
           var lambdaOutput = _getDocumentsIds.Execute();
           LambdaLogger.Log("Document ids retrieved" + JsonConvert.SerializeObject(lambdaOutput));
        }
    }
}
