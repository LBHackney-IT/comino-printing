using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace UseCases.GatewayInterfaces
{
    public interface ISqsGateway
    {
        Task<SendMessageBatchResponse> AddDocumentIdsToQueue(List<string> documentsIds);
    }
}