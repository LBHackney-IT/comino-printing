namespace Usecases.Interfaces
{
    public interface IParseHtmlToPdf
    {
        void Execute(string html, string filePath, int marginTop = 5, int marginRight = 15, int marginBottom = 5,
            int marginLeft = 15);
    }
}