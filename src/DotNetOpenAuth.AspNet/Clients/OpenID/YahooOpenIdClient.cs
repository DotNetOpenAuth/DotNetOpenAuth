namespace DotNetOpenAuth.AspNet.Clients {
	using System.Collections.Generic;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public sealed class YahooOpenIdClient : OpenIDClient {
		public YahooOpenIdClient() :
			base("yahoo", "http://me.yahoo.com") {
		}

		/// <summary>
		/// Called just before the authentication request is sent to service provider.
		/// </summary>
		/// <param name="request">The request.</param>
		protected override void OnBeforeSendingAuthenticationRequest(IAuthenticationRequest request) {
			// Attribute Exchange extensions
			var fetchRequest = new FetchRequest();
			fetchRequest.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.Email, isRequired: true));
			fetchRequest.Attributes.Add(new AttributeRequest(WellKnownAttributes.Name.FullName, isRequired: false));

			request.AddExtension(fetchRequest);
		}

		/// <summary>
		/// Gets the extra data obtained from the response message when authentication is successful.
		/// </summary>
		/// <param name="response">The response message.</param>
		/// <returns></returns>
		protected override Dictionary<string, string> GetExtraData(IAuthenticationResponse response) {
			FetchResponse fetchResponse = response.GetExtension<FetchResponse>();
			if (fetchResponse != null) {
				var extraData = new Dictionary<string, string>();
				extraData.AddItemIfNotEmpty("email", fetchResponse.GetAttributeValue(WellKnownAttributes.Contact.Email));
				extraData.AddItemIfNotEmpty("fullName", fetchResponse.GetAttributeValue(WellKnownAttributes.Name.FullName));

				return extraData;
			}

			return null;
		}
	}
}
