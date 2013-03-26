//-----------------------------------------------------------------------
// <copyright file="NegativeAssertionResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// The message OpenID Providers send back to Relying Parties to refuse
	/// to assert the identity of a user.
	/// </summary>
	[Serializable]
	internal class NegativeAssertionResponse : IndirectResponseBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="NegativeAssertionResponse"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="relyingPartyReturnTo">The relying party return to.</param>
		/// <param name="mode">The value of the openid.mode parameter.</param>
		internal NegativeAssertionResponse(Version version, Uri relyingPartyReturnTo, string mode)
			: base(version, relyingPartyReturnTo, mode) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NegativeAssertionResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal NegativeAssertionResponse(SignedResponseRequest request)
			: base(request, GetMode(request)) {
		}

		/// <summary>
		/// Gets or sets the URL the relying party can use to upgrade their authentication
		/// request from an immediate to a setup message.
		/// </summary>
		/// <value>URL to redirect User-Agent to so the End User can do whatever's necessary to fulfill the assertion.</value>
		/// <remarks>
		/// This part is only included in OpenID 1.x responses.
		/// </remarks>
		[MessagePart("openid.user_setup_url", AllowEmpty = false, IsRequired = false, MaxVersion = "1.1")]
		internal Uri UserSetupUrl { get; set; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="NegativeAssertionResponse"/>
		/// is in response to an authentication request made in immediate mode.
		/// </summary>
		/// <value><c>true</c> if the request was in immediate mode; otherwise, <c>false</c>.</value>
		internal bool Immediate {
			get {
				if (this.OriginatingRequest != null) {
					return this.OriginatingRequest.Immediate;
				} else {
					if (string.Equals(this.Mode, Protocol.Args.Mode.setup_needed, StringComparison.Ordinal)) {
						return true;
					} else if (string.Equals(this.Mode, Protocol.Args.Mode.cancel, StringComparison.Ordinal)) {
						return false;
					} else {
						throw ErrorUtilities.ThrowProtocol(MessagingStrings.UnexpectedMessagePartValue, Protocol.openid.mode, this.Mode);
					}
				}
			}
		}

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

			// Since there are a couple of negative assertion modes, ensure that the mode given is one of the allowed ones.
			ErrorUtilities.VerifyProtocol(string.Equals(this.Mode, Protocol.Args.Mode.setup_needed, StringComparison.Ordinal) || string.Equals(this.Mode, Protocol.Args.Mode.cancel, StringComparison.Ordinal), MessagingStrings.UnexpectedMessagePartValue, Protocol.openid.mode, this.Mode);

			if (this.Immediate && Protocol.Version.Major < 2) {
				ErrorUtilities.VerifyProtocol(this.UserSetupUrl != null, OpenIdStrings.UserSetupUrlRequiredInImmediateNegativeResponse);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NegativeAssertionResponse" /> class.
		/// </summary>
		/// <param name="request">The request that the relying party sent.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="channel">The channel to use to simulate construction of the user_setup_url, if applicable.  May be null, but the user_setup_url will not be constructed.</param>
		/// <returns>The negative assertion message that will indicate failure for the user to authenticate or an unwillingness to log into the relying party.</returns>
		internal static async Task<NegativeAssertionResponse> CreateAsync(SignedResponseRequest request, CancellationToken cancellationToken, Channel channel = null) {
			var result = new NegativeAssertionResponse(request);

			// If appropriate, and when we're provided with a channel to do it,
			// go ahead and construct the user_setup_url
			if (result.Version.Major < 2 && request.Immediate && channel != null) {
				// All requests are CheckIdRequests in OpenID 1.x, so this cast should be safe.
				result.UserSetupUrl = await ConstructUserSetupUrlAsync((CheckIdRequest)request, channel, cancellationToken);
			}

			return result;
		}

		/// <summary>
		/// Constructs the value for the user_setup_url parameter to be sent back
		/// in negative assertions in response to OpenID 1.x RP's checkid_immediate requests.
		/// </summary>
		/// <param name="immediateRequest">The immediate request.</param>
		/// <param name="channel">The channel to use to simulate construction of the message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The value to use for the user_setup_url parameter.</returns>
		private static async Task<Uri> ConstructUserSetupUrlAsync(CheckIdRequest immediateRequest, Channel channel, CancellationToken cancellationToken) {
			Requires.NotNull(immediateRequest, "immediateRequest");
			Requires.NotNull(channel, "channel");
			ErrorUtilities.VerifyInternal(immediateRequest.Immediate, "Only immediate requests should be sent here.");

			var setupRequest = new CheckIdRequest(immediateRequest.Version, immediateRequest.Recipient, AuthenticationRequestMode.Setup);
			setupRequest.LocalIdentifier = immediateRequest.LocalIdentifier;
			setupRequest.ReturnTo = immediateRequest.ReturnTo;
			setupRequest.Realm = immediateRequest.Realm;
			setupRequest.AssociationHandle = immediateRequest.AssociationHandle;
			var response = await channel.PrepareResponseAsync(setupRequest, cancellationToken);
			return response.GetDirectUriRequest();
		}

		/// <summary>
		/// Gets the value for the openid.mode that is appropriate for this response.
		/// </summary>
		/// <param name="request">The request that we're responding to.</param>
		/// <returns>The value of the openid.mode parameter to use.</returns>
		private static string GetMode(SignedResponseRequest request) {
			Requires.NotNull(request, "request");

			Protocol protocol = Protocol.Lookup(request.Version);
			return request.Immediate ? protocol.Args.Mode.setup_needed : protocol.Args.Mode.cancel;
		}
	}
}
