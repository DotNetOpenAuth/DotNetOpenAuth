//-----------------------------------------------------------------------
// <copyright file="AuthorizationRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.OAuth {
	using System;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An extension to include with an authentication request in order to also 
	/// obtain authorization to access user data at the combined OpenID Provider
	/// and Service Provider.
	/// </summary>
	/// <remarks>
	/// <para>When requesting OpenID Authentication via the protocol mode "checkid_setup" 
	/// or "checkid_immediate", this extension can be used to request that the end 
	/// user authorize an OAuth access token at the same time as an OpenID 
	/// authentication. This is done by sending the following parameters as part 
	/// of the OpenID request. (Note that the use of "oauth" as part of the parameter 
	/// names here and in subsequent sections is just an example. See Section 5 for details.)</para>
	/// <para>See section 8.</para>
	/// </remarks>
	[Serializable]
	public class AuthorizationRequest : ExtensionBase {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly StandardOpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage, isProviderRole) => {
			if (typeUri == Constants.TypeUri && isProviderRole) {
				return new AuthorizationRequest();
			}

			return null;
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationRequest"/> class.
		/// </summary>
		public AuthorizationRequest()
			: base(new Version(1, 0), Constants.TypeUri, null) {
		}

		/// <summary>
		/// Gets or sets the consumer key agreed upon between the Consumer and Service Provider.
		/// </summary>
		[MessagePart("consumer", IsRequired = true, AllowEmpty = false)]
		public string Consumer { get; set; }

		/// <summary>
		/// Gets or sets a string that encodes, in a way possibly specific to the Combined Provider, one or more scopes for the OAuth token expected in the authentication response.
		/// </summary>
		[MessagePart("scope", IsRequired = false)]
		public string Scope { get; set; }
	}
}
