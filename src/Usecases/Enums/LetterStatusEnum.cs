namespace Usecases.Enums
{
    public enum LetterStatusEnum
    {
        Waiting, Processing, ProcessingError, WaitingForApproval, ReadyForGovNotify, PrintedManually, SentToGovNotify, FailedToSend ,GovNotifyPendingVirusCheck, GovNotifyVirusScanFailed, GovNotifyValidationFailed, LetterSent, Cancelled
    }
}
