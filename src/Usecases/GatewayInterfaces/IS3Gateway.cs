namespace UseCases.GatewayInterfaces
{
    public interface IS3Gateway
    {
        string SavePdfDocument(string documentId, string filename);
    }
}