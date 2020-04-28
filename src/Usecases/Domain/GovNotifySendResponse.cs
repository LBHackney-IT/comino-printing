namespace Usecases.Domain
{
    public class GovNotifySendResponse
    {
        public string NotificationId { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}