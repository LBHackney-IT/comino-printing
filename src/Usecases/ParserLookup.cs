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
                default:
                    return null;
            }

        }
    }
}