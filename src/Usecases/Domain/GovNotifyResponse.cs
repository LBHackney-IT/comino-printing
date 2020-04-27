using Usecases.Enums;

namespace Usecases.Domain
{
    public class GovNotifyResponse
    {
        public GovNotifyStatusEnum Status { get; set; }
        public string SentAt { get; set; }
    }
}