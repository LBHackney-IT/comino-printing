namespace Usecases.Enums
{
    public enum LetterStatusEnum
    {
        Waiting, Processing, ProcessingError, WaitingForApproval, ReadyForGovNotify, SentToGovNotify, FailedToSend ,GovNotifyPendingVirusCheck, GovNotifyVirusScanFailed, GovNotifyValidationFailed, LetterSent, Cancelled
    }
}
