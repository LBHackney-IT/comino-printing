using System;
using System.Collections.Generic;
using Usecases.Domain;

namespace UseCases.GatewayInterfaces
{
    public interface IW2DocumentsGateway
    {
        string GetHtmlDocument(string documentId);
    }
}
