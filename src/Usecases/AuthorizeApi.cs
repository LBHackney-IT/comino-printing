using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Boundary.UseCaseInterfaces;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Usecases
{
    public class AuthorizeApi : IAuthorizeApi
    {
        public APIGatewayCustomAuthorizerResponse Execute(APIGatewayCustomAuthorizerRequest request)
        {
            if (request.AuthorizationToken == null)
            {
                return AccessDenied(request);
            }

            var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
            var authorisedGroup = Environment.GetEnvironmentVariable("ALLOWED_USER_GROUP");
            var token = request.AuthorizationToken.Replace("Bearer ", "");
            var decodedToken = DecodeToken(secret, token);

            if (!decodedToken.Identity.IsAuthenticated) return AccessDenied(request);

            var groups = decodedToken.Claims.Where(c => c.Type == "groups").ToList();

            if (groups.Any(grp => grp.Value == authorisedGroup))
            {
                return AccessAuthorized(request);
            }
            return AccessDenied(request);
        }

        private static ClaimsPrincipal DecodeToken(string secret, string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false
            };
            var handler = new JwtSecurityTokenHandler();

            return handler.ValidateToken(token, tokenValidationParameters, out _);
        }

        private static APIGatewayCustomAuthorizerResponse AccessAuthorized(APIGatewayCustomAuthorizerRequest request)
        {
            return ConstructResponse(request, true);

        }

        private static APIGatewayCustomAuthorizerResponse AccessDenied(APIGatewayCustomAuthorizerRequest request)
        {
            return ConstructResponse(request, false);
        }

        private static APIGatewayCustomAuthorizerResponse ConstructResponse(APIGatewayCustomAuthorizerRequest request, bool authorized)
        {
            return new APIGatewayCustomAuthorizerResponse
            {
                PrincipalID = "user",
                PolicyDocument = new APIGatewayCustomAuthorizerPolicy
                {
                    Version = "2012-10-17",
                    Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
                    {
                        new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                        {
                            Effect = authorized ? "Allow" : "Deny",
                            Resource = new HashSet<string> {request.MethodArn},
                            Action = new HashSet<string> {"execute-api: Invoke"},
                        }
                    }
                }
            };
        }
    }
}