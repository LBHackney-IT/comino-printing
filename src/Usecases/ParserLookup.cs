using Usecases.UntestedParsers;
using Usecases.UseCaseInterfaces;

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
                    return new BlankBenefitsRtfParser();
                default:
                    return null;
            }

        }
    }
}