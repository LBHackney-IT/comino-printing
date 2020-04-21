using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;

namespace Usecases.UseCaseInterfaces
{
    public interface IProcessEvents
    {
        Task Execute(SQSEvent sqsEvent);
    }
}
