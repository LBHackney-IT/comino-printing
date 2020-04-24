using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Usecases.Interfaces;

namespace Gateways
{
    public class HtmlToPdfConversionGateway : IParseHtmlToPdf
    {
        private readonly string _baseUrl;
        private readonly HttpClient _client;
        public HtmlToPdfConversionGateway(HttpClient client)
        {
            _client = client;
            _baseUrl = Environment.GetEnvironmentVariable("HTML_TO_PDF_CONVERSION_ENDPOINT");
        }
        public async Task<byte[]> Convert(string fullHtml, string documentId)
        {
            if (_baseUrl == null)
            {
                Console.WriteLine("Endpoint URL to convert from HTML to PDF not set");
            }

            Console.WriteLine("Calling conversion endpoint");

            var response = await _client.PostAsync(_baseUrl, new StringContent(fullHtml));

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Conversion endpoint returned unsuccessful");
                throw new Exception("Conversion endpoint returned unsuccessful");
            }

            Console.WriteLine("Conversion endpoint returned successful");
            var content = await response.Content.ReadAsByteArrayAsync();
            return content;
        }
    }
}
