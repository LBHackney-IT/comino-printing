namespace Usecases.Enums
{
    public enum LetterStatusEnum
    {
        Waiting, Processing, ProcessingError, WaitingForApproval, ReadyForGovNotify, SentToGovNotify, GovNotifyPendingVirusCheck, GovNotifyVirusScanFailed, GovNotifyValidationFailed, LetterSent, Cancelled
    }
}
