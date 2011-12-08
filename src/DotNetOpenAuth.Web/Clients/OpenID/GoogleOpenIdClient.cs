using System.Collections.Generic;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace DotNetOpenAuth.Web.Clients
{
    /// <summary>
    /// Represents Google OpenID client.
    /// </summary>
    internal sealed class GoogleOpenIdClient : OpenIDClient
    {
        public GoogleOpenIdClient() :
            base("google", "https://www.google.com/accounts/o8/id")
        {
        }

        /// <summary>
        /// Called just before the authentication request is sent to service provider.
        /// </summary>
        /// <param name="request">The request.</param>
        protected override void OnBeforeSendingAuthenticationRequest(IAuthenticationRequest request)
        {
            // Attribute Exchange extensions
            var fetchRequest = new FetchRequest();
            fetchRequest.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.Email, isRequired: true));
            fetchRequest.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.HomeAddress.Country, isRequired: false));
            fetchRequest.Attributes.Add(new AttributeRequest(AxKnownAttributes.FirstName, isRequired: false));
            fetchRequest.Attributes.Add(new AttributeRequest(AxKnownAttributes.LastName, isRequired: false));

            request.AddExtension(fetchRequest);
        }

        /// <summary>
        /// Gets the extra data obtained from the response message when authentication is successful.
        /// </summary>
        /// <param name="response">The response message.</param>
        /// <returns></returns>
        protected override Dictionary<string, string> GetExtraData(IAuthenticationResponse response)
        {
            FetchResponse fetchResponse = response.GetExtension<FetchResponse>();
            if (fetchResponse != null)
            {
                var extraData = new Dictionary<string, string>();
                extraData.AddItemIfNotEmpty("email", fetchResponse.GetAttributeValue(WellKnownAttributes.Contact.Email));
                extraData.AddItemIfNotEmpty("country", fetchResponse.GetAttributeValue(WellKnownAttributes.Contact.HomeAddress.Country));
                extraData.AddItemIfNotEmpty("firstName", fetchResponse.GetAttributeValue(AxKnownAttributes.FirstName));
                extraData.AddItemIfNotEmpty("lastName", fetchResponse.GetAttributeValue(AxKnownAttributes.LastName));

                return extraData;
            }

            return null;
        }
    }
}