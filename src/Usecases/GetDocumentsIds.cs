using System;
using System.Collections.Generic;
using AwsDotnetCsharp.UsecaseInterfaces;
using Usecases.Domain;
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

        public List<DocumentDetails> Execute()
        {
            var documents = _cominoGateway.GetDocumentsAfterStartDate(DateTime.Now.AddMinutes(-1));
        }
    }
}
