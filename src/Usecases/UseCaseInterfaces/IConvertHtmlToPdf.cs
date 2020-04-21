namespace Usecases.UseCaseInterfaces
{
    public interface IConvertHtmlToPdf
    {
        byte[] Execute(string htmlDocument, string documentType);
    }
}