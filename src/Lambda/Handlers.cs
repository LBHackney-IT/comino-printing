using System;
// using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Boundary.UseCaseInterfaces;
// using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        public async Task ConvertDocumentToPdf(SQSEvent sqsEvent, ILambdaContext context)
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

        public async Task QueryDocumentsAndSendToNotify(ILambdaContext context)
        {
            var queryDocsForNotifyUseCase = _serviceProvider.GetService<IQueryDocumentsAndSendToNotify>();
            try
            {
                await queryDocsForNotifyUseCase.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
