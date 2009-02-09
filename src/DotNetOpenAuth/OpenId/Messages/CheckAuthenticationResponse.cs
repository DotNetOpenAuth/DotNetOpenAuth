//-----------------------------------------------------------------------
// <copyright file="CheckAuthenticationResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// The message sent from the Provider to the Relying Party to confirm/deny
	/// the validity of an assertion that was signed by a private Provider secret.
	/// </summary>
	internal class CheckAuthenticationResponse : DirectResponseBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckAuthenticationResponse"/> class
		/// for use by the Relying Party.
		/// </summary>
		/// <param name="responseVersion">The OpenID version of the response message.</param>
		/// <param name="request">The request that this message is responding to.</param>
		internal CheckAuthenticationResponse(Version responseVersion, CheckAuthenticationRequest request)
			: base(responseVersion, request) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CheckAuthenticationResponse"/> class
		/// for use by the Provider.
		/// </summary>
		/// <param name="request">The request that this message is responding to.</param>
		/// <param name="provider">The OpenID Provider that is preparing to send this response.</param>
		internal CheckAuthenticationResponse(CheckAuthenticationRequest request, OpenIdProvider provider)
			: base(request.Version, request) {
			ErrorUtilities.VerifyArgumentNotNull(provider, "provider");

			// The channel's binding elements have already set the request's IsValid property
			// appropriately.  We just copy it into the response message.
			this.IsValid = request.IsValid;

			// Confirm the RP should invalidate the association handle only if the association
			// really doesn't exist.  OpenID 2.0 section 11.4.2.2.
			IndirectSignedResponse signedResponse = new IndirectSignedResponse(request);
			string invalidateHandle = ((ITamperResistantOpenIdMessage)signedResponse).InvalidateHandle;
			if (invalidateHandle != null && provider.AssociationStore.GetAssociation(AssociationRelyingPartyType.Smart, invalidateHandle) == null) {
				this.InvalidateHandle = invalidateHandle;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the signature of the verification request is valid.
		/// </summary>
		[MessagePart("is_valid", IsRequired = true)]
		internal bool IsValid { get; set; }

		/// <summary>
		/// Gets or sets the handle the relying party should invalidate if <see cref="IsValid"/> is true.
		/// </summary>
		/// <value>The "invalidate_handle" value sent in the verification request, if the OP confirms it is invalid.</value>
		/// <remarks>
		/// <para>If present in a verification response with "is_valid" set to "true",
		/// the Relying Party SHOULD remove the corresponding association from 
		/// its store and SHOULD NOT send further authentication requests with 
		/// this handle.</para>
		/// <para>This two-step process for invalidating associations is necessary 
		/// to prevent an attacker from invalidating an association at will by 
		/// adding "invalidate_handle" parameters to an authentication response.</para>
		/// </remarks>
		[MessagePart("invalidate_handle", IsRequired = false, AllowEmpty = false)]
		internal string InvalidateHandle { get; set; }
	}
}
