using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using AutoFixture;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NUnit.Framework;
using Usecases;

namespace UnitTests
{
    public class AuthorizeApiTests
    {
        private Fixture _fixtue;
        private string _testSecret;
        private string _currentGroups;
        private string _currentSecret;
        private string _testGroup;
        private AuthorizeApi _subject;
        private JwtSecurityTokenHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _fixtue = new Fixture();
            _currentSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
            _currentGroups = Environment.GetEnvironmentVariable("ALLOWED_USER_GROUP");
            _testSecret = _fixtue.Create<string>();
            _testGroup = _fixtue.Create<string>();
            Environment.SetEnvironmentVariable("JWT_SECRET", _testSecret);
            Environment.SetEnvironmentVariable("ALLOWED_USER_GROUP", _testGroup);

            _handler = new JwtSecurityTokenHandler();
            _subject = new AuthorizeApi();
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("JWT_SECRET", _currentSecret);
            Environment.SetEnvironmentVariable("ALLOWED_USER_GROUP", _currentGroups);
        }

        [Test]
        public void IfTokenIsNull_ReturnsUnauthorised()
        {
            var request = _fixtue.Create<APIGatewayCustomAuthorizerRequest>();
            request.AuthorizationToken = null;

            var expectedPolicy = UnauthorizedPolicyForArn(request.MethodArn);
            _subject.Execute(request).Should().BeEquivalentTo(expectedPolicy);
        }

        [Test]
        public void IfTokenIsValid_ReturnsAuthorized()
        {
            var key = Encoding.UTF8.GetBytes(_testSecret);
            var token = _handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("groups", _fixtue.Create<string>()),
                    new Claim("groups", _testGroup),
                }),
                Expires = DateTime.Now.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var request = _fixtue.Create<APIGatewayCustomAuthorizerRequest>();
            request.AuthorizationToken = $"Bearer {_handler.WriteToken(token)}";

            var expectedPolicy = AuthorizedPolicyForArn(request.MethodArn);
            _subject.Execute(request).Should().BeEquivalentTo(expectedPolicy);
        }

        [Test]
        public void IfValidTokenIsInTheHeaders_ReturnsAuthorized()
        {
            var key = Encoding.UTF8.GetBytes(_testSecret);
            var token = _handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("groups", _fixtue.Create<string>()),
                    new Claim("groups", _testGroup),
                }),
                Expires = DateTime.Now.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var request = _fixtue.Create<APIGatewayCustomAuthorizerRequest>();
            request.AuthorizationToken = null;
            request.Headers.Add(new KeyValuePair<string, string>("Authorization",  $"Bearer {_handler.WriteToken(token)}"));

            var expectedPolicy = AuthorizedPolicyForArn(request.MethodArn);
            _subject.Execute(request).Should().BeEquivalentTo(expectedPolicy);
        }

        [Test]
        public void IfValidTokenIsInTheQueryStringParameters_ReturnsAuthorized()
        {
            var key = Encoding.UTF8.GetBytes(_testSecret);
            var token = _handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("groups", _fixtue.Create<string>()),
                    new Claim("groups", _testGroup),
                }),
                Expires = DateTime.Now.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var request = _fixtue.Create<APIGatewayCustomAuthorizerRequest>();
            request.AuthorizationToken = null;
            request.QueryStringParameters.Add(new KeyValuePair<string, string>("authToken", _handler.WriteToken(token)));

            var expectedPolicy = AuthorizedPolicyForArn(request.MethodArn);
            _subject.Execute(request).Should().BeEquivalentTo(expectedPolicy);
        }

        [Test]
        public void IfTokenIsInCookies_ReturnsAuthorized()
        {
            var key = Encoding.UTF8.GetBytes(_testSecret);
            var token = _handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("groups", _fixtue.Create<string>()),
                    new Claim("groups", _testGroup),
                }),
                Expires = DateTime.Now.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var request = _fixtue.Create<APIGatewayCustomAuthorizerRequest>();
            request.AuthorizationToken = null;
            var cookieHeader = $"hackneyToken={_handler.WriteToken(token)};";
            request.Headers.Add(new KeyValuePair<string, string>("Cookie", cookieHeader));

            var expectedPolicy = AuthorizedPolicyForArn(request.MethodArn);
            _subject.Execute(request).Should().BeEquivalentTo(expectedPolicy);
        }

        [Test]
        public void IfTokenIsMissingGroups_ReturnsUnauthorized()
        {
            var key = Encoding.UTF8.GetBytes(_testSecret);
            var token = _handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(),
                Expires = DateTime.Now.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            });

            var request = _fixtue.Create<APIGatewayCustomAuthorizerRequest>();
            request.AuthorizationToken = _handler.WriteToken(token);

            var expectedPolicy = UnauthorizedPolicyForArn(request.MethodArn);
            _subject.Execute(request).Should().BeEquivalentTo(expectedPolicy);
        }

        [Test]
        public void IfUserIsNotInCorrectGroup_ReturnsUnauthorized()
        {
            var key = Encoding.UTF8.GetBytes(_testSecret);
            var wrongGroups = JsonConvert.SerializeObject(_fixtue.CreateMany<string>().ToList());
            var token = _handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("groups", wrongGroups)
                }),
                Expires = DateTime.Now.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            });

            var request = _fixtue.Create<APIGatewayCustomAuthorizerRequest>();
            request.AuthorizationToken = _handler.WriteToken(token);

            var expectedPolicy = UnauthorizedPolicyForArn(request.MethodArn);
            _subject.Execute(request).Should().BeEquivalentTo(expectedPolicy);
        }

        private APIGatewayCustomAuthorizerResponse UnauthorizedPolicyForArn(string arn)
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
                            Effect = "Deny",
                            Resource = new HashSet<string> {arn},
                            Action = new HashSet<string> {"execute-api:Invoke"},
                        }
                    }
                }
            };
        }

        private APIGatewayCustomAuthorizerResponse AuthorizedPolicyForArn(string arn)
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
                            Effect = "Allow",
                            Resource = new HashSet<string> {arn},
                            Action = new HashSet<string> {"execute-api:Invoke"},
                        }
                    }
                }
            };
        }
    }
}
