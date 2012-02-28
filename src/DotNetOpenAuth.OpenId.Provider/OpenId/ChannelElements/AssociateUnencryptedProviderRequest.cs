//-----------------------------------------------------------------------
// <copyright file="AssociateUnencryptedProviderRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Represents an association request received by the OpenID Provider that is sent using HTTPS and 
	/// otherwise communicates the shared secret in plain text.
	/// </summary>
	internal class AssociateUnencryptedProviderRequest : AssociateUnencryptedRequest, IAssociateRequestProvider {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateUnencryptedProviderRequest"/> class.
		/// </summary>
		/// <param name="version">The OpenID version this message must comply with.</param>
		/// <param name="providerEndpoint">The OpenID Provider endpoint.</param>
		internal AssociateUnencryptedProviderRequest(Version version, Uri providerEndpoint)
			: base(version, providerEndpoint) {
		}

		/// <summary>
		/// Creates a Provider's response to an incoming association request.
		/// </summary>
		/// <returns>
		/// The appropriate association response message.
		/// </returns>
		public IProtocolMessage CreateResponseCore() {
			var response = new AssociateUnencryptedResponseProvider(this.Version, this);
			response.AssociationType = this.AssociationType;
			return response;
		}
	}
}
