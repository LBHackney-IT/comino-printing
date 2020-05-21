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
                default:
                    return null;
            }

        }
    }
}