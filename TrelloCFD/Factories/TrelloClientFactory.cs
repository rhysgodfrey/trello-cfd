using Chello.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrelloCFD.Configuration;

namespace TrelloCFD.Factories
{
    public static class TrelloClientFactory
    {
        public static ChelloClient Get()
        {
            string apiKey = TrelloConfigurationManager.ApiKey;
            string userToken = HttpContext.Current.User.Identity.Name;

            return new ChelloClient(apiKey, userToken);
        }
    }
}