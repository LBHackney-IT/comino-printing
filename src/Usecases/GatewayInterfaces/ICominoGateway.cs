using System;
using System.Collections.Generic;

namespace UseCases.GatewayInterfaces
{
    public interface ICominoGateway
    {
        List<string> GetDocumentsAfterStartDate(DateTime time);
    }
}
