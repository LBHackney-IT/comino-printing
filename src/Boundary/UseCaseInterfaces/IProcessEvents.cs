using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;

namespace Boundary.UseCaseInterfaces
{
    public interface IProcessEvents
    {
        Task Execute(SQSEvent sqsEvent);
    }
}
