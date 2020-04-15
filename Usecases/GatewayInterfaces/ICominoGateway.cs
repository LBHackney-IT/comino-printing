using System;
using System.Collections.Generic;

namespace Usecases.GatewayInterfaces
{
    public interface ICominoGateway
    {
        List<string> GetDocumentsAfterStartDate(DateTime time);
    }
}