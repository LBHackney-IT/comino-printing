using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        public W2DocumentsGateway(HttpClient client)
        {
            _client = client;
            _baseUrl = Environment.GetEnvironmentVariable("W2_DOCUMENT_BASE_URL");
        }

        public async Task<string> GetHtmlDocument(string documentId)
        {
            var response = await _client.GetAsync(_baseUrl + $"/hncomino/documents/{documentId}/view");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Cannot retrieve tasks");
            }

            var content = await response.Content.ReadAsStringAsync();
            var html = JsonConvert.DeserializeObject<string>(content);

            return html;
        }
    }
}
