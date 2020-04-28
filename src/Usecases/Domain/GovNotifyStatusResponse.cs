using Usecases.Enums;

namespace Usecases.Domain
{
    public class GovNotifyStatusResponse
    {
        public LetterStatusEnum Status { get; set; }
        public string SentAt { get; set; }
    }
}