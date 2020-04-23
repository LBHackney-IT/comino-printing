namespace Boundary.UseCaseInterfaces
{
    public interface IUpdateDocuments
    {
        void Execute(string id, string status);
    }
}