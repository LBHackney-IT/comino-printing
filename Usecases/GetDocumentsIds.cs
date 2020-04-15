using System;
using System.Collections.Generic;
using AwsDotnetCsharp.UsecaseInterfaces;
using UseCases.GatewayInterfaces;

namespace UseCases
{
    public class GetDocumentsIds : IGetDocumentsIds
    {
        private readonly ICominoGateway _cominoGateway;

        public GetDocumentsIds(ICominoGateway cominoGateway)
        {
            _cominoGateway = cominoGateway;
        }

        public List<string> Execute()
        {
            return _cominoGateway.GetDocumentsAfterStartDate(DateTime.Now.AddMinutes(-1));
        }
    }
}
