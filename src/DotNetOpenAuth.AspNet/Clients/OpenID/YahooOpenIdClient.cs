//-----------------------------------------------------------------------
// <copyright file="YahooOpenIdClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System.Collections.Generic;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// The yahoo open id client.
	/// </summary>
	public sealed class YahooOpenIdClient : OpenIdClient {
		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="YahooOpenIdClient"/> class.
		/// </summary>
		public YahooOpenIdClient()
			: base("yahoo", WellKnownProviders.Yahoo) { }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the extra data obtained from the response message when authentication is successful.
		/// </summary>
		/// <param name="response">
		/// The response message. 
		/// </param>
		/// <returns>A dictionary of profile data; or null if no data is available.</returns>
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

		/// <summary>
		/// Called just before the authentication request is sent to service provider.
		/// </summary>
		/// <param name="request">
		/// The request. 
		/// </param>
		protected override void OnBeforeSendingAuthenticationRequest(IAuthenticationRequest request) {
			// Attribute Exchange extensions
			var fetchRequest = new FetchRequest();
			fetchRequest.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
			fetchRequest.Attributes.AddOptional(WellKnownAttributes.Name.FullName);

			request.AddExtension(fetchRequest);
		}

		#endregion
	}
}
