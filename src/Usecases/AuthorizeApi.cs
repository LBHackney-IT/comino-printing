using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Boundary.UseCaseInterfaces;
using Microsoft.IdentityModel.Tokens;

namespace Usecases
{
    public class AuthorizeApi : IAuthorizeApi
    {
        public APIGatewayCustomAuthorizerResponse Execute(APIGatewayCustomAuthorizerRequest request)
        {
            var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
            var token = request.AuthorizationToken;
            Console.Write(token);
            
            var allowedGroupName = Environment.GetEnvironmentVariable("ALLOWED_USER_GROUP");

            var decodedToken = DecodeToken(secret, token);

            if (decodedToken == null)
            {
                return AccessDenied(request);
            }
            else
            {
                if (decodedToken.Identity.IsAuthenticated && decodedToken.IsInRole(allowedGroupName))
                {
                    return AccessAuthorized(request);
                }
                else
                {
                    foreach (var claim in decodedToken.Claims)
                    {
                        Console.Write("CLAIM TYPE: " + claim.Type + "; CLAIM VALUE: " + claim.Value + "</br>");
                    }

                    return AccessDenied(request);
                }
            }
        }

        private static ClaimsPrincipal DecodeToken(string secret, string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
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
                            Action = new HashSet<string> {"execute-api", "Invoke"},
                        }
                    }
                }
            };
        }
    }
}