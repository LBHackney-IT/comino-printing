namespace Usecases.UseCaseInterfaces
{
    public interface IGetParser
    {
        ILetterParser ForType(string letterType);
    }
}