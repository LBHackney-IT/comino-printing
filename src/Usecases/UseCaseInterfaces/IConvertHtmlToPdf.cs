using System.IO;

namespace Usecases.UseCaseInterfaces
{
    public interface IConvertHtmlToPdf
    {
        void Execute(string htmlDocument, string documentType, string documentId);
    }
}