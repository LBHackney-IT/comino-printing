using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
using Notify.Client;


namespace Gateways
{
    public class GovNotifyHandler : IGovNotifyHandler
    {
        public GovNotifyHandler()
        {
            var apiKey = Environment.GetEnvironmentVariable("GOV_NOTIFY_API_KEY");
            var client = new NotificationClient(apiKey);
        }

        public string GovNotifyClient { get; }
    }
}
