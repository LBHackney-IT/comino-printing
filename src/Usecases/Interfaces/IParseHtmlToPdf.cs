using System.Threading.Tasks;

namespace Usecases.Interfaces
{
    public interface IParseHtmlToPdf
    {
        Task<byte[]> Convert(string html, string documentId);
    }
}