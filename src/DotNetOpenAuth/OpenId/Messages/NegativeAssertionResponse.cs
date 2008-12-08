//-----------------------------------------------------------------------
// <copyright file="NegativeAssertionResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The message OpenID Providers send back to Relying Parties to refuse
	/// to assert the identity of a user.
	/// </summary>
	internal class NegativeAssertionResponse : IndirectResponseBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="NegativeAssertionResponse"/> class.
		/// </summary>
		/// <param name="request">The request that the relying party sent.</param>
		internal NegativeAssertionResponse(CheckIdRequest request)
			: base(request, GetMode(request)) {
		}

		/// <summary>
		/// Gets or sets the URL the relying party can use to upgrade their authentication
		/// request from an immediate to a setup message.
		/// </summary>
		/// <value>URL to redirect User-Agent to so the End User can do whatever's necessary to fulfill the assertion.</value>
		/// <remarks>
		/// This part is only included 
		/// </remarks>
		[MessagePart("openid.user_setup_url", AllowEmpty = false, IsRequired = false, MaxVersion = "1.1")]
		internal Uri UserSetupUrl { get; set; }

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// 	<para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the
		/// message to see if it conforms to the protocol.</para>
		/// 	<para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		public override void EnsureValidMessage() {
			base.EnsureValidMessage();

			if (OriginatingRequest.Immediate && Protocol.Version.Major < 2) {
				ErrorUtilities.VerifyProtocol(this.UserSetupUrl != null, OpenIdStrings.UserSetupUrlRequiredInImmediateNegativeResponse);
			}
		}

		/// <summary>
		/// Gets the value for the openid.mode that is appropriate for this response.
		/// </summary>
		/// <param name="request">The request that we're responding to.</param>
		/// <returns>The value of the openid.mode parameter to use.</returns>
		private static string GetMode(CheckIdRequest request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			Protocol protocol = Protocol.Lookup(request.ProtocolVersion);
			return request.Immediate ? protocol.Args.Mode.setup_needed : protocol.Args.Mode.cancel;
		}
	}
}
