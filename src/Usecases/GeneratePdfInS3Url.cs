using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class GeneratePdfInS3Url 
    {
        private IS3Gateway _s3Gateway;

        public GeneratePdfInS3Url(IS3Gateway s3Gateway)
        {
            _s3Gateway = s3Gateway;
        }

        public void Execute(string idFromRequest)
        {
            
        }
    }
}