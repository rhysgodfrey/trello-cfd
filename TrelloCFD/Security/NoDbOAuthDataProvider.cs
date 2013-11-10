using DotNetOpenAuth.AspNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using WebMatrix.WebData;

namespace TrelloCFD.Security
{
    public class NoDbOAuthDataProvider : IOpenAuthDataProvider
    {
        private static ExtendedMembershipProvider VerifyProvider()
        {
            var provider = Membership.Provider as ExtendedMembershipProvider;
            if (provider == null)
            {
                throw new InvalidOperationException("No extended membership provider");
            }
            return provider;
        }

        public string GetUserNameFromOpenAuth(string openAuthProvider, string openAuthId)
        {
            ExtendedMembershipProvider provider = VerifyProvider();

            if (openAuthProvider.Equals("trello", StringComparison.OrdinalIgnoreCase))
            {
                return openAuthId;
            }

            int userId = provider.GetUserIdFromOAuth(openAuthProvider, openAuthId);
            if (userId == -1)
            {
                return null;
            }

            return provider.GetUserNameFromId(userId);
        }
    }
}