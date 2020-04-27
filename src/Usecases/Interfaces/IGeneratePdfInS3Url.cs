namespace Usecases.Interfaces
{
    public interface IGeneratePdfInS3Url
    {
        string Execute(string id);
    }
}