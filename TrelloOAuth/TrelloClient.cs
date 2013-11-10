using Chello.Core;
using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloOAuth
{
    public class TrelloClient : OAuthClient
    {
        private const string _providerName = "trello";
        private string _consumerKey;

        /// <summary>
        /// Describes the OAuth service provider endpoints for Trello.
        /// </summary>
        public static readonly ServiceProviderDescription TrelloServiceDescription = new ServiceProviderDescription
        {
            RequestTokenEndpoint =
                new MessageReceivingEndpoint(
                    "https://trello.com/1/OAuthGetRequestToken",
                    HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            UserAuthorizationEndpoint =
                new MessageReceivingEndpoint(
                    "https://trello.com/1/OAuthAuthorizeToken",
                    HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            AccessTokenEndpoint =
                new MessageReceivingEndpoint(
                    "https://trello.com/1/OAuthGetAccessToken",
                    HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
            TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="TrelloClient"/> class.
        /// </summary>
        /// <param name="consumerKey">The consumer key. This is your Trello API key.</param>
        /// <param name="consumerSecret">The consumer secret. This is your Trello API Secret.</param>
        public TrelloClient(string consumerKey, string consumerSecret)
            : this(consumerKey, consumerSecret, new AuthenticationOnlyCookieOAuthTokenManager()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrelloClient"/> class.
        /// </summary>
        /// <param name="consumerKey">The consumer key. This is your Trello API key.</param>
        /// <param name="consumerSecret">The consumer secret. This is your Trello API Secret.</param>
        /// <param name="tokenManager">The token manager.</param>
        public TrelloClient(string consumerKey, string consumerSecret, IOAuthTokenManager tokenManager)
            : base(_providerName, TrelloServiceDescription, new SimpleConsumerTokenManager(consumerKey, consumerSecret, tokenManager))
        {
            _consumerKey = consumerKey;
        }

        protected override AuthenticationResult VerifyAuthenticationCore(AuthorizedTokenResponse response)
        {
            try
            {
                if (response != null && !String.IsNullOrWhiteSpace(response.AccessToken))
                {
                    ChelloClient client = new ChelloClient(_consumerKey, response.AccessToken);
                    Member member = client.Members.Single("me");

                    response.ExtraData.Add("url", member.Url);
                    response.ExtraData.Add("gravatar", member.Gravatar);
                    response.ExtraData.Add("username", member.Username);
                    response.ExtraData.Add("bio", member.Bio);
                    response.ExtraData.Add("fullName", member.FullName);

                    return new AuthenticationResult(true, _providerName, response.AccessToken, member.Username, response.ExtraData);
                }

                return new AuthenticationResult(false);
            }
            catch (Exception ex)
            {
                return new AuthenticationResult(ex, _providerName);
            }
        }
    }
}
