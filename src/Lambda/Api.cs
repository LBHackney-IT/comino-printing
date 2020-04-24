using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Boundary.UseCaseInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Usecases.Interfaces;

namespace AwsDotnetCsharp
{
    public class Api
    {
        private readonly IGetAllDocuments _getAllDocumentsUseCase;
        private IUpdateDocumentState _updateDocumentUsecase;

        public Api()
        {
            var services = new ServiceCollection();
            services.Configure();
            var serviceProvider = services.BuildServiceProvider();
            _getAllDocumentsUseCase = serviceProvider.GetService<IGetAllDocuments>();
        }

        public async Task<APIGatewayProxyResponse> GetAllDocuments(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var limit = request.QueryStringParameters["limit"];
            var cursor = request.QueryStringParameters["cursor"];
            var documents = await _getAllDocumentsUseCase.Execute(limit, cursor);
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = ConvertToCamelCasedJson(documents),
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}, {"Access-Control-Allow-Origin", "*"}},
            };

            return response;
        }

        public APIGatewayProxyResponse GetById(APIGatewayProxyRequest request, ILambdaContext context)
        {
//            Get s3 pdf Url from somewhere
//            use Response.Redirect("")to redirect to the pdf in s3;
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                //TODO
                Body = "Use Case Response Here",
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}, {"Access-Control-Allow-Origin", "*"}}
            };

            return response;
        }

        public async Task<APIGatewayProxyResponse> ApproveDocument(APIGatewayProxyRequest request, ILambdaContext context)
        {
//            _updateDocuments.Execute(id, status);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Headers = new Dictionary<string, string>{{"Access-Control-Allow-Origin", "*"}}
            };

            return response;
        }
        
        public async Task<APIGatewayProxyResponse> UpdateDocumentStatus(string id, string status )
        {   
            // get status from the request from API gateway
            // await _updateDocumentUsecase.Execute(id, status);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Headers = new Dictionary<string, string>{{"Access-Control-Allow-Origin", "*"}}
            };

            return response;
        }

        private static string ConvertToCamelCasedJson<T>(T responseBody)
        {
            return JsonConvert.SerializeObject(responseBody, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            });
        }
    }
}