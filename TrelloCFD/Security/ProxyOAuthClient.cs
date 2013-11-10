using DotNetOpenAuth.AspNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrelloCFD.Security
{
    public class ProxyOAuthClient : IAuthenticationClient
    {
        private string _name;

        public ProxyOAuthClient(string name)
        {
            _name = name;
        }

        public string ProviderName
        {
            get { return _name; }
        }

        public void RequestAuthentication(HttpContextBase context, Uri returnUrl)
        {
            throw new NotImplementedException();
        }

        public AuthenticationResult VerifyAuthentication(HttpContextBase context)
        {
            throw new NotImplementedException();
        }
    }
}