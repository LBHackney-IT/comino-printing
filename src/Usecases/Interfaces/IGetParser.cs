namespace Usecases.Interfaces
{
    public interface IGetParser
    {
        ILetterParser ForType(string letterType);
    }
}