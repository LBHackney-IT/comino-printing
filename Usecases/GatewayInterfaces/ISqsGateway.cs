using System.Collections.Generic;

namespace UseCases.GatewayInterfaces
{
    public interface ISqsGateway
    {
        void AddDocumentIdsToQueue(List<string> documentsIds);
    }
}