using Usecases.Enums;

namespace Usecases
{
    public static class LettersStatusExtensions
    {
        public static string PrettierStatusName(this LetterStatusEnum status)
        {
            switch (status)
            {
                case LetterStatusEnum.GovNotifyPendingVirusCheck:
                    return "Pending Virus Check";
                case LetterStatusEnum.GovNotifyVirusScanFailed:
                    return "Virus Scan Failed";
                case LetterStatusEnum.GovNotifyValidationFailed:
                    return "Validation Failed";
                case LetterStatusEnum.LetterSent:
                    return "Letter Sent";
                case LetterStatusEnum.ProcessingError:
                    return "Processing Error";
                case LetterStatusEnum.WaitingForApproval:
                    return "Waiting for Approval";
                case LetterStatusEnum.ReadyForGovNotify:
                    return "Ready for Gov Notify";
                case LetterStatusEnum.SentToGovNotify:
                    return "Sent to Gov Notify";
                default:
                    return status.ToString();
            }
        }
    }
}