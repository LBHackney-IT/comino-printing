using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Boundary.UseCaseInterfaces;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Usecases
{
    public class AuthorizeApi : IAuthorizeApi
    {
        public APIGatewayCustomAuthorizerResponse Execute(APIGatewayCustomAuthorizerRequest request)
        {
            var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
            var authorisedGroup = Environment.GetEnvironmentVariable("ALLOWED_USER_GROUP");
            var token = FindToken(request);

            if (token == null)
            {
                return AccessDenied(request);
            }

            var decodedToken = DecodeToken(secret, token);

            if (!decodedToken.Identity.IsAuthenticated) return AccessDenied(request);

            var groups = decodedToken.Claims.Where(c => c.Type == "groups").ToList();

            if (groups.Any(grp => grp.Value == authorisedGroup))
            {
                return AccessAuthorized(request);
            }
            return AccessDenied(request);
        }

        private static string FindToken(APIGatewayCustomAuthorizerRequest request)
        {
            var authorizationHeader = GetAuthorizationHeaderValue(request);
            if (authorizationHeader != null) return RemoveBearer(request.Headers["Authorization"]);

            var queryAuthToken = GetAuthTokenFromQuery(request);
            if (queryAuthToken != null)  return GetAuthTokenFromQuery(request);

            var cookie = GetCookieValueFromRequest(request, "hackneyToken");
            if (cookie != null) return cookie;

            return request.AuthorizationToken != null
                ? RemoveBearer(request.AuthorizationToken)
                : null;
        }

        private static string GetAuthorizationHeaderValue(APIGatewayCustomAuthorizerRequest request)
        {
            if (request.Headers == null) return null;
            return request.Headers.ContainsKey("Authorization")
                ? request.Headers["Authorization"]
                : null;
        }

        private static string GetAuthTokenFromQuery(APIGatewayCustomAuthorizerRequest request)
        {
            if (request.QueryStringParameters == null) return null;
            return request.QueryStringParameters.ContainsKey("authToken")
                ? request.QueryStringParameters["authToken"]
                : null;
        }

        private static string GetCookieValueFromRequest(APIGatewayCustomAuthorizerRequest request, string cookieName)
        {
            if (request.Headers == null) return null;
            if (!request.Headers.ContainsKey("Cookie")) return null;
            var responseHeaders = request.Headers.Where(header => header.Key == "Cookie").Select(header => header.Value).ToList();
            return (
                from header in responseHeaders
                where header.Trim().StartsWith($"{cookieName}=")
                let p1 = header.IndexOf('=')
                let p2 = header.IndexOf(';')
                select header.Substring(p1 + 1, p2 - p1 - 1)).FirstOrDefault();
        }

        private static string RemoveBearer(string token)
        {
            return token.Replace("Bearer ", "");
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
                            Action = new HashSet<string> {"execute-api:Invoke"},
                        }
                    }
                }
            };
        }
    }
}