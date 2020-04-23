using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Usecases.UseCaseInterfaces;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handlers
    {
        private readonly ServiceProvider _serviceProvider;

        public Handlers()
        {
            var services = new ServiceCollection();
            services.Configure();
            _serviceProvider = services.BuildServiceProvider();
        }

        public void FetchAndQueueDocumentIds(ILambdaContext context)
        {
            var getDocumentsUseCse = _serviceProvider.GetService<IFetchAndQueueDocumentIds>();
            getDocumentsUseCse.Execute();
        }

        public async Task ListenForSqsEvents(SQSEvent sqsEvent, ILambdaContext context)
        {
            var listenForSqsEventsUseCase = _serviceProvider.GetService<IProcessEvents>();
            await listenForSqsEventsUseCase.Execute(sqsEvent);
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
