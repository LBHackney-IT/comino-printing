namespace Usecases.UseCaseInterfaces
{
    public interface IParseHtmlToPdf
    {
        byte[] Execute(string html, int marginTop = 5, int marginRight = 15, int marginBottom = 5, int marginLeft = 15);
    }
}