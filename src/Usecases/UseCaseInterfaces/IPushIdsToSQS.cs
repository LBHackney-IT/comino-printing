using System.Collections.Generic;
using Amazon.SQS.Model;

namespace AwsDotnetCsharp.UsecaseInterfaces
{
    public interface IPushIdsToSqs
    {
        List<SendMessageResponse> Execute(List<string> documentIds);
    }
}