using Usecases.Enums;

namespace Usecases.Domain
{
    public class GovNotifyResponse
    {
        public LetterStatusEnum Status { get; set; }
        public string SentAt { get; set; }
    }
}