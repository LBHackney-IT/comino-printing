using System;
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

        public async Task FetchAndQueueDocumentIds(ILambdaContext context)
        {
            var getDocumentsUseCase = _serviceProvider.GetService<IFetchAndQueueDocumentIds>();
            try
            {
                await getDocumentsUseCase.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task ListenForSqsEvents(SQSEvent sqsEvent, ILambdaContext context)
        {
            var listenForSqsEventsUseCase = _serviceProvider.GetService<IProcessEvents>();
            try
            {
                await listenForSqsEventsUseCase.Execute(sqsEvent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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
