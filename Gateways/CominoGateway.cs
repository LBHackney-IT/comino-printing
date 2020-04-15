using System;
using System.Collections.Generic;
using UseCases.GatewayInterfaces;

namespace Gateways
{
    public class CominoGateway : ICominoGateway
    {
        public List<string> GetDocumentsAfterStartDate(DateTime time)
        {
            return new List<string>();
        }
    }
}