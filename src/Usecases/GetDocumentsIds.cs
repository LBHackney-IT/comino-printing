using System;
using System.Collections.Generic;
using Usecases.Domain;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

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
            var timeSpanInMinutes = Convert.ToInt32(Environment.GetEnvironmentVariable("DOCUMENTS_QUERY_TIMESPAN_MINUTES") ?? "1");
            var startTime = DateTime.Now.AddMinutes(-timeSpanInMinutes);
            Console.WriteLine($"Querying database for letter since {startTime}");
            return _cominoGateway.GetDocumentsAfterStartDate(startTime);
        }
    }
}
