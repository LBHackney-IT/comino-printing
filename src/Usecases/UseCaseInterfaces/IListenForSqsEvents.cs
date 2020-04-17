using Amazon.Lambda.SQSEvents;
using System.Collections.Generic;

namespace AwsDotnetCsharp.UsecaseInterfaces
{
    public interface IListenForSqsEvents
    {
        List<string> Execute(SQSEvent sqsEvent);
    }
}
