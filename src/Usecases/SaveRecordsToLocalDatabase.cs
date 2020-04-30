using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Usecases.Domain;
using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

namespace UseCases
{
    public class SaveRecordsToLocalDatabase : ISaveRecordsToLocalDatabase
    {
        private readonly ILocalDatabaseGateway _databaseGateway;

        public SaveRecordsToLocalDatabase(ILocalDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public async Task<List<DocumentDetails>> Execute(List<DocumentDetails> documentsToSave)
        {
            List<DocumentDetails> savedDocuments = new List<DocumentDetails>();
            foreach (var document in documentsToSave)
            {
                var saved = await _databaseGateway.SaveDocument(document);
                if(saved){
                    document.Id = document.Date;
                    savedDocuments.Add(document);
                }
            }

            return savedDocuments.ToList();
        }
    }
}
