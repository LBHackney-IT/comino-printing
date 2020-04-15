using Amazon.Lambda.Core;
using System;
using System.Threading;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class GetDocuments
    {
       public void FetchDocumentIds(ILambdaContext context)
       {
         Console.Write(context.RemainingTime);
         Thread.Sleep(500);
         Console.Write(context.RemainingTime);
         Thread.Sleep(500);
         Console.Write(context.RemainingTime);
         Thread.Sleep(500);
       }
    }
}
