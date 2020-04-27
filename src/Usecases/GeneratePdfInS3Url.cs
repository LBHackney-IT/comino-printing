using UseCases.GatewayInterfaces;
using Usecases.Interfaces;

namespace UseCases
{
    public class GeneratePdfInS3Url : IGeneratePdfInS3Url
    {
        private IS3Gateway _s3Gateway;

        public GeneratePdfInS3Url(IS3Gateway s3Gateway)
        {
            _s3Gateway = s3Gateway;
        }

        public string Execute(string id)
        {
            return _s3Gateway.GeneratePdfUrl(id);
        }
    }
}