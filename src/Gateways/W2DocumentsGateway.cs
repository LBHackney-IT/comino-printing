using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
// using System.IO;
// using System.Net;
// using System.Threading;
// using System.Text;
using UseCases.GatewayInterfaces;

namespace Gateways
{
    public class W2DocumentsGateway : IW2DocumentsGateway
    {
        private readonly string _baseUrl;
        private readonly HttpClient _client;
        private readonly string _token;

        public W2DocumentsGateway(HttpClient client)
        {
            _client = client;
            _baseUrl = Environment.GetEnvironmentVariable("W2_DOCUMENT_BASE_URL");
            _token = Environment.GetEnvironmentVariable("DOCUMENTS_API_TOKEN");
        }

        public async Task<string> GetHtmlDocument(string documentId)
        {
            var urlToGet = _baseUrl + $"/hncomino/documents/{documentId}/view";
            Console.WriteLine($"Get HTML from {urlToGet}");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.GetAsync(urlToGet);

            if (!response.IsSuccessStatusCode)
            {
                Console.Write(JsonConvert.SerializeObject(response));
                throw new Exception("Cannot retrieve html");
            }

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}
