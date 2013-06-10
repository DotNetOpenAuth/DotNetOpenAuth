//-----------------------------------------------------------------------
// <copyright file="CheckAuthenticationResponseProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Provider;
	using Validation;

	/// <summary>
	/// The check_auth response message, as it is seen by the OpenID Provider.
	/// </summary>
	internal class CheckAuthenticationResponseProvider : CheckAuthenticationResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckAuthenticationResponseProvider"/> class.
		/// </summary>
		/// <param name="request">The request that this message is responding to.</param>
		/// <param name="provider">The OpenID Provider that is preparing to send this response.</param>
		internal CheckAuthenticationResponseProvider(CheckAuthenticationRequest request, OpenIdProvider provider)
			: base(request.Version, request) {
			Requires.NotNull(provider, "provider");

			// The channel's binding elements have already set the request's IsValid property
			// appropriately.  We just copy it into the response message.
			this.IsValid = request.IsValid;

			// Confirm the RP should invalidate the association handle only if the association
			// is not valid (any longer).  OpenID 2.0 section 11.4.2.2.
			IndirectSignedResponse signedResponse = new IndirectSignedResponse(request, provider.Channel);
			string invalidateHandle = ((ITamperResistantOpenIdMessage)signedResponse).InvalidateHandle;
			if (!string.IsNullOrEmpty(invalidateHandle) && !provider.AssociationStore.IsValid(signedResponse, false, invalidateHandle)) {
				this.InvalidateHandle = invalidateHandle;
			}
		}
	}
}
