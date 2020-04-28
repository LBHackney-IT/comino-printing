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
        private readonly IApproveDocument _approveDocumentUseCase;
        private readonly IGeneratePdfInS3Url _generatePdfInS3UrlUseCase;
        private readonly IGetSingleDocumentInfo _getSingleDocumentInfoUseCase;
        private readonly ICancelDocument _cancelDocumentUseCase;

        public Api()
        {
            var services = new ServiceCollection();
            services.Configure();
            var serviceProvider = services.BuildServiceProvider();
            _getAllDocumentsUseCase = serviceProvider.GetService<IGetAllDocuments>();
            _approveDocumentUseCase = serviceProvider.GetService<IApproveDocument>();
            _generatePdfInS3UrlUseCase = serviceProvider.GetService<IGeneratePdfInS3Url>();
            _getSingleDocumentInfoUseCase = serviceProvider.GetService<IGetSingleDocumentInfo>();
            _cancelDocumentUseCase = serviceProvider.GetService<ICancelDocument>();
        }

        public async Task<APIGatewayProxyResponse> GetAllDocuments(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var limit = request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey("limit")
                ? request.QueryStringParameters["limit"]
                : "10";

            var cursor = request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey("cursor")
                ? request.QueryStringParameters["cursor"]
                : null;

            var documents = await _getAllDocumentsUseCase.Execute(limit, cursor);
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = ConvertToCamelCasedJson(documents),
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}, {"Access-Control-Allow-Origin", "*"}},
            };

            return response;
        }

        public async Task<APIGatewayProxyResponse> GetById(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var id = request.PathParameters["id"];
            Console.Write(id);
            var documentInfo = await _getSingleDocumentInfoUseCase.Execute(id);
            
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = ConvertToCamelCasedJson(documentInfo),
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}, {"Access-Control-Allow-Origin", "*"}}
            };

            return response;
        }

        public async Task<APIGatewayProxyResponse> ApproveDocument(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var id = request.PathParameters["id"];
            await _approveDocumentUseCase.Execute(id);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Headers = new Dictionary<string, string>{{"Access-Control-Allow-Origin", "*"}}
            };

            return response;
        }
        
        public async Task<APIGatewayProxyResponse> CancelDocument(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var id = request.PathParameters["id"];
            await _cancelDocumentUseCase.Execute(id);
            
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Headers = new Dictionary<string, string>{{"Access-Control-Allow-Origin", "*"}}
            };
            
            return response;
        }

        public APIGatewayProxyResponse ViewDocumentPdf(APIGatewayProxyRequest request, ILambdaContext context)
        {
            
            var id = request.PathParameters["id"];
            var redirectUrl = _generatePdfInS3UrlUseCase.Execute(id);
            
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Redirect,
                Headers = new Dictionary<string, string>{ { "location", redirectUrl } }
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