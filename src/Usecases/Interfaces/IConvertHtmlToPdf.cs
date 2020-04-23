namespace Usecases.Interfaces
{
    public interface IConvertHtmlToPdf
    {
        void Execute(string htmlDocument, string documentType, string documentId);
    }
}