namespace AwsDotnetCsharp.UsecaseInterfaces
{
    public interface ISavePdfToS3
    {
        string Execute(string documentId, string filename);
    }
}