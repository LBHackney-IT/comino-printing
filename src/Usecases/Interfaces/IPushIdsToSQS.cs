using System.Collections.Generic;
using Amazon.SQS.Model;

namespace Usecases.Interfaces
{
    public interface IPushIdsToSqs
    {
        List<SendMessageResponse> Execute(List<string> documentIds);
    }
}