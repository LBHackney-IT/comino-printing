using Amazon.Lambda.SQSEvents;

namespace Usecases.UseCaseInterfaces
{
    public interface IProcessEvents
    {
        void Execute(SQSEvent sqsEvent);
    }
}
