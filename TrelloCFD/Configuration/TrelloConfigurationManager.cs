using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloCFD.Configuration
{
    public static class TrelloConfigurationManager
    {
        public static string ApiKey
        {
            get
            {
                var apiKey = ConfigurationManager.AppSettings["TrelloApiKey"];

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new ArgumentNullException("Trello API Key must be supplied see https://github.com/rhysgodfrey/trello-cfd/blob/master/README.md for more information");
                }

                return apiKey;
            }
        }

        public static string ApiSecret
        {
            get
            {
                var apiSecret = ConfigurationManager.AppSettings["TrelloApiSecret"];

                if (string.IsNullOrWhiteSpace(apiSecret))
                {
                    throw new ArgumentNullException("Trello API Secret must be supplied see https://github.com/rhysgodfrey/trello-cfd/blob/master/README.md for more information");
                }

                return apiSecret;
            }
        }
    }
}
