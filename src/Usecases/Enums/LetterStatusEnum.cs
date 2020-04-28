namespace Usecases.Enums
{
    public enum LetterStatusEnum
    {
        Waiting, Processing, ProcessingError, WaitingForApproval, ReadyForGovNotify, SentToGovNotify, GovNotifySendError ,GovNotifyPendingVirusCheck, GovNotifyVirusScanFailed, GovNotifyValidationFailed, LetterSent, Cancelled
    }
}
