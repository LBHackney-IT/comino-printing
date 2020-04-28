using Amazon.Lambda.APIGatewayEvents;

namespace Boundary.UseCaseInterfaces
{
    public interface IAuthorizeApi
    {
        APIGatewayCustomAuthorizerResponse Execute(APIGatewayCustomAuthorizerRequest request);
    }
}