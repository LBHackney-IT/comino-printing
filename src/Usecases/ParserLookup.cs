using Usecases.Interfaces;
using Usecases.UntestedParsers;

namespace UseCases
{
    public class ParserLookup : IGetParser
    {
        public ILetterParser ForType(string letterType)
        {
            switch (letterType)
            {
                case "Change in Circs ICL":
                    return new ChangesInCircsICL();
                case "Benefits Blank Letter":
                    return new BlankBenefitsDetails();
                case "HB/CTR Invite to Claim":
                    return new HBCTRInviteToClaim();
                case "Benefit ICL":
                    return new BenefitICL();
                case "Review ICL":
                    return new ReviewICL();
                case "Adverse Inference (BEN 999)":
                    return new AdverseInferenceBEN999();
                case "Revision-decision upheld":
                    return new RevisionDecisionUpheld();
                case "Backdate Accepted":
                    return new BackdateAccepted();
                case "Revision-decision revised":
                    return new RevisionDecisionRevised();
                case "Backdate Refused":
                    return new BackdateRefused();
                case "Late Notification":
                    return new LateNotification();
                case "Benefit Suspend Letter":
                    return new BenefitSuspendLetter();
                case "Invite To Claim CTR - Claimed HB via DWP":
                    return new InviteToClaimCTRClaimedHBviaDWP();
                default:
                    return null;
            }

        }
    }
}