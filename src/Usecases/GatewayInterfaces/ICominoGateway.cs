using System;
using System.Collections.Generic;
using Usecases.Domain;

namespace UseCases.GatewayInterfaces
{
    public interface ICominoGateway
    {
        List<DocumentDetails> GetDocumentsAfterStartDate(DateTime time);
        void MarkDocumentAsSent(string documentNumber);
        CominoSentStatusCheck GetDocumentSentStatus(string cominoDocumentNumber);
    }
}
