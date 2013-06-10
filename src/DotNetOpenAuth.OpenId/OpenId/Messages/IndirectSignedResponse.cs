//-----------------------------------------------------------------------
// <copyright file="IndirectSignedResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Net.Security;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using Validation;

	/// <summary>
	/// An indirect message from a Provider to a Relying Party where at least part of the
	/// payload is signed so the Relying Party can verify it has not been tampered with.
	/// </summary>
	[DebuggerDisplay("OpenID {Version} {Mode} (no id assertion)")]
	[Serializable]
	[SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1630:DocumentationTextMustContainWhitespace", Justification = "The samples are string literals.")]
	[SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1631:DocumentationMustMeetCharacterPercentage", Justification = "The samples are string literals.")]
	internal class IndirectSignedResponse : IndirectResponseBase, ITamperResistantOpenIdMessage {
		/// <summary>
		/// The allowed date/time formats for the response_nonce parameter.
		/// </summary>
		/// <remarks>
		/// This array of formats is not yet a complete list.
		/// </remarks>
		private static readonly string[] PermissibleDateTimeFormats = { "yyyy-MM-ddTHH:mm:ssZ" };

		/// <summary>
		/// Backing field for the <see cref="IExpiringProtocolMessage.UtcCreationDate"/> property.
		/// </summary>
		/// <remarks>
		/// The field initializer being DateTime.UtcNow allows for OpenID 1.x messages
		/// to pass through the StandardExpirationBindingElement.
		/// </remarks>
		private DateTime creationDateUtc = DateTime.UtcNow;

		/// <summary>
		/// Backing store for the <see cref="ReturnToParameters"/> property.
		/// </summary>
		private IDictionary<string, string> returnToParameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="IndirectSignedResponse"/> class.
		/// </summary>
		/// <param name="request">
		/// The authentication request that caused this assertion to be generated.
		/// </param>
		internal IndirectSignedResponse(SignedResponseRequest request)
			: base(request, Protocol.Lookup(GetVersion(request)).Args.Mode.id_res) {
			Requires.NotNull(request, "request");

			this.ReturnTo = request.ReturnTo;
			this.ProviderEndpoint = request.Recipient.StripQueryArgumentsWithPrefix(Protocol.openid.Prefix);
			((ITamperResistantOpenIdMessage)this).AssociationHandle = request.AssociationHandle;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndirectSignedResponse"/> class
		/// in order to perform signature verification at the Provider.
		/// </summary>
		/// <param name="previouslySignedMessage">The previously signed message.</param>
		/// <param name="channel">The channel.  This is used only within the constructor and is not stored in a field.</param>
		internal IndirectSignedResponse(CheckAuthenticationRequest previouslySignedMessage, Channel channel)
			: base(GetVersion(previouslySignedMessage), previouslySignedMessage.ReturnTo, Protocol.Lookup(GetVersion(previouslySignedMessage)).Args.Mode.id_res) {
			Requires.NotNull(channel, "channel");

			// Copy all message parts from the check_authentication message into this one,
			// except for the openid.mode parameter.
			MessageDictionary checkPayload = channel.MessageDescriptions.GetAccessor(previouslySignedMessage);
			MessageDictionary thisPayload = channel.MessageDescriptions.GetAccessor(this);
			foreach (var pair in checkPayload) {
				if (!string.Equals(pair.Key, this.Protocol.openid.mode)) {
					thisPayload[pair.Key] = pair.Value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndirectSignedResponse"/> class
		/// for unsolicited assertions.
		/// </summary>
		/// <param name="version">The OpenID version to use.</param>
		/// <param name="relyingPartyReturnTo">The return_to URL of the Relying Party.
		/// This value will commonly be from <see cref="SignedResponseRequest.ReturnTo"/>,
		/// but for unsolicited assertions may come from the Provider performing RP discovery
		/// to find the appropriate return_to URL to use.</param>
		internal IndirectSignedResponse(Version version, Uri relyingPartyReturnTo)
			: base(version, relyingPartyReturnTo, Protocol.Lookup(version).Args.Mode.id_res) {
			this.ReturnTo = relyingPartyReturnTo;
		}

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		/// <value>
		/// <see cref="MessageProtections.All"/> for OpenID 2.0 messages.
		/// <see cref="MessageProtections.TamperProtection"/> for OpenID 1.x messages.
		/// </value>
		/// <remarks>
		/// Although the required protection is reduced for OpenID 1.x,
		/// this library will provide Relying Party hosts with all protections
		/// by adding its own specially-crafted nonce to the authentication request
		/// messages except for stateless RPs in OpenID 1.x messages.
		/// </remarks>
		public override MessageProtections RequiredProtection {
			// We actually manage to provide All protections regardless of OpenID version
			// on both the Provider and Relying Party side, except for stateless RPs for OpenID 1.x.
			get { return this.Version.Major < 2 ? MessageProtections.TamperProtection : MessageProtections.All; }
		}

		/// <summary>
		/// Gets or sets the message signature.
		/// </summary>
		/// <value>Base 64 encoded signature calculated as specified in Section 6 (Generating Signatures).</value>
		[MessagePart("openid.sig", IsRequired = true, AllowEmpty = false)]
		string ITamperResistantProtocolMessage.Signature { get; set; }

		/// <summary>
		/// Gets or sets the signed parameter order.
		/// </summary>
		/// <value>Comma-separated list of signed fields.</value>
		/// <example>"op_endpoint,identity,claimed_id,return_to,assoc_handle,response_nonce"</example>
		/// <remarks>
		/// This entry consists of the fields without the "openid." prefix that the signature covers.
		/// This list MUST contain at least "op_endpoint", "return_to" "response_nonce" and "assoc_handle",
		/// and if present in the response, "claimed_id" and "identity".
		/// Additional keys MAY be signed as part of the message. See Generating Signatures.
		/// </remarks>
		[MessagePart("openid.signed", IsRequired = true, AllowEmpty = false)]
		string ITamperResistantOpenIdMessage.SignedParameterOrder { get; set; }

		/// <summary>
		/// Gets or sets the association handle used to sign the message.
		/// </summary>
		/// <value>The handle for the association that was used to sign this assertion. </value>
		[MessagePart("openid.assoc_handle", IsRequired = true, AllowEmpty = false, RequiredProtection = ProtectionLevel.Sign, MinVersion = "2.0")]
		[MessagePart("openid.assoc_handle", IsRequired = true, AllowEmpty = false, RequiredProtection = ProtectionLevel.None, MaxVersion = "1.1")]
		string ITamperResistantOpenIdMessage.AssociationHandle { get; set; }

		/// <summary>
		/// Gets or sets the nonce that will protect the message from replay attacks.
		/// </summary>
		string IReplayProtectedProtocolMessage.Nonce { get; set; }

		/// <summary>
		/// Gets the context within which the nonce must be unique.
		/// </summary>
		string IReplayProtectedProtocolMessage.NonceContext {
			get {
				if (this.ProviderEndpoint != null) {
					return this.ProviderEndpoint.AbsoluteUri;
				} else {
					// This is the Provider, on an OpenID 1.x check_authentication message.
					// We don't need any special nonce context because the Provider
					// generated and consumed the nonce.
					return string.Empty;
				}
			}
		}

		/// <summary>
		/// Gets or sets the UTC date/time the message was originally sent onto the network.
		/// </summary>
		/// <remarks>
		/// The property setter should ensure a UTC date/time,
		/// and throw an exception if this is not possible.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown when a DateTime that cannot be converted to UTC is set.
		/// </exception>
		DateTime IExpiringProtocolMessage.UtcCreationDate {
			get { return this.creationDateUtc; }
			set { this.creationDateUtc = value.ToUniversalTimeSafe(); }
		}

		/// <summary>
		/// Gets or sets the association handle that the Provider wants the Relying Party to not use any more.
		/// </summary>
		/// <value>If the Relying Party sent an invalid association handle with the request, it SHOULD be included here.</value>
		/// <remarks>
		/// For OpenID 1.1, we allow this to be present but empty to put up with poor implementations such as Blogger.
		/// </remarks>
		[MessagePart("openid.invalidate_handle", IsRequired = false, AllowEmpty = true, MaxVersion = "1.1")]
		[MessagePart("openid.invalidate_handle", IsRequired = false, AllowEmpty = false, MinVersion = "2.0")]
		string ITamperResistantOpenIdMessage.InvalidateHandle { get; set; }

		/// <summary>
		/// Gets or sets the Provider Endpoint URI.
		/// </summary>
		[MessagePart("openid.op_endpoint", IsRequired = true, AllowEmpty = false, RequiredProtection = ProtectionLevel.Sign, MinVersion = "2.0")]
		internal Uri ProviderEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the return_to parameter as the relying party provided
		/// it in <see cref="SignedResponseRequest.ReturnTo"/>.
		/// </summary>
		/// <value>Verbatim copy of the return_to URL parameter sent in the
		/// request, before the Provider modified it. </value>
		[MessagePart("openid.return_to", IsRequired = true, AllowEmpty = false, RequiredProtection = ProtectionLevel.Sign, Encoder = typeof(OriginalStringUriEncoder))]
		internal Uri ReturnTo { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the <see cref="ReturnTo"/>
		/// URI's query string is unaltered between when the Relying Party
		/// sent the original request and when the response was received.
		/// </summary>
		/// <remarks>
		/// This property is not persisted in the transmitted message, and
		/// has no effect on the Provider-side of the communication.
		/// </remarks>
		internal bool ReturnToParametersSignatureValidated { get; set; }

		/// <summary>
		/// Gets or sets the nonce that will protect the message from replay attacks.
		/// </summary>
		/// <value>
		/// <para>A string 255 characters or less in length, that MUST be unique to 
		/// this particular successful authentication response. The nonce MUST start 
		/// with the current time on the server, and MAY contain additional ASCII 
		/// characters in the range 33-126 inclusive (printable non-whitespace characters), 
		/// as necessary to make each response unique. The date and time MUST be 
		/// formatted as specified in section 5.6 of [RFC3339] 
		/// (Klyne, G. and C. Newman, “Date and Time on the Internet: Timestamps,” .), 
		/// with the following restrictions:</para>
		/// <list type="bullet">
		///   <item>All times must be in the UTC timezone, indicated with a "Z".</item>
		///   <item>No fractional seconds are allowed</item>
		/// </list>
		/// </value>
		/// <example>2005-05-15T17:11:51ZUNIQUE</example>
		internal string ResponseNonceTestHook {
			get { return this.ResponseNonce; }
			set { this.ResponseNonce = value; }
		}

		/// <summary>
		/// Gets or sets the nonce that will protect the message from replay attacks.
		/// </summary>
		/// <value>
		/// <para>A string 255 characters or less in length, that MUST be unique to 
		/// this particular successful authentication response. The nonce MUST start 
		/// with the current time on the server, and MAY contain additional ASCII 
		/// characters in the range 33-126 inclusive (printable non-whitespace characters), 
		/// as necessary to make each response unique. The date and time MUST be 
		/// formatted as specified in section 5.6 of [RFC3339] 
		/// (Klyne, G. and C. Newman, “Date and Time on the Internet: Timestamps,” .), 
		/// with the following restrictions:</para>
		/// <list type="bullet">
		///   <item>All times must be in the UTC timezone, indicated with a "Z".</item>
		///   <item>No fractional seconds are allowed</item>
		/// </list>
		/// </value>
		/// <example>2005-05-15T17:11:51ZUNIQUE</example>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by messaging framework via reflection.")]
		[MessagePart("openid.response_nonce", IsRequired = true, AllowEmpty = false, RequiredProtection = ProtectionLevel.Sign, MinVersion = "2.0")]
		[MessagePart("openid.response_nonce", IsRequired = false, AllowEmpty = false, RequiredProtection = ProtectionLevel.None, MaxVersion = "1.1")]
		private string ResponseNonce {
			get {
				string uniqueFragment = ((IReplayProtectedProtocolMessage)this).Nonce;
				return this.creationDateUtc.ToString(PermissibleDateTimeFormats[0], CultureInfo.InvariantCulture) + uniqueFragment;
			}

			set {
				if (value == null) {
					((IReplayProtectedProtocolMessage)this).Nonce = null;
				} else {
					int indexOfZ = value.IndexOf("Z", StringComparison.Ordinal);
					ErrorUtilities.VerifyProtocol(indexOfZ >= 0, MessagingStrings.UnexpectedMessagePartValue, Protocol.openid.response_nonce, value);
					this.creationDateUtc = DateTime.Parse(value.Substring(0, indexOfZ + 1), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
					((IReplayProtectedProtocolMessage)this).Nonce = value.Substring(indexOfZ + 1);
				}
			}
		}

		/// <summary>
		/// Gets the querystring key=value pairs in the return_to URL.
		/// </summary>
		private IDictionary<string, string> ReturnToParameters {
			get {
				if (this.returnToParameters == null) {
					this.returnToParameters = HttpUtility.ParseQueryString(this.ReturnTo.Query).ToDictionary();
				}

				return this.returnToParameters;
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

			this.VerifyReturnToMatchesRecipient();
		}

		/// <summary>
		/// Gets the value of a named parameter in the return_to URL without signature protection.
		/// </summary>
		/// <param name="key">The full name of the parameter whose value is being sought.</param>
		/// <returns>The value of the parameter if it is present and unaltered from when
		/// the Relying Party signed it; <c>null</c> otherwise.</returns>
		/// <remarks>
		/// This method will always return null on the Provider-side, since Providers
		/// cannot verify the private signature made by the relying party.
		/// </remarks>
		internal string GetReturnToArgument(string key) {
			Requires.NotNullOrEmpty(key, "key");
			ErrorUtilities.VerifyInternal(this.ReturnTo != null, "ReturnTo was expected to be required but is null.");

			string value;
			this.ReturnToParameters.TryGetValue(key, out value);
			return value;
		}

		/// <summary>
		/// Gets the names of the callback parameters added to the original authentication request
		/// without signature protection.
		/// </summary>
		/// <returns>A sequence of the callback parameter names.</returns>
		internal IEnumerable<string> GetReturnToParameterNames() {
			return this.ReturnToParameters.Keys;
		}

		/// <summary>
		/// Gets a dictionary of all the message part names and values
		/// that are included in the message signature.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <returns>
		/// A dictionary of the signed message parts.
		/// </returns>
		internal IDictionary<string, string> GetSignedMessageParts(Channel channel) {
			Requires.NotNull(channel, "channel");

			ITamperResistantOpenIdMessage signedSelf = this;
			if (signedSelf.SignedParameterOrder == null) {
				return EmptyDictionary<string, string>.Instance;
			}

			MessageDictionary messageDictionary = channel.MessageDescriptions.GetAccessor(this);
			string[] signedPartNamesWithoutPrefix = signedSelf.SignedParameterOrder.Split(',');
			Dictionary<string, string> signedParts = new Dictionary<string, string>(signedPartNamesWithoutPrefix.Length);

			var signedPartNames = signedPartNamesWithoutPrefix.Select(part => Protocol.openid.Prefix + part);
			foreach (string partName in signedPartNames) {
				signedParts[partName] = messageDictionary[partName];
			}

			return signedParts;
		}

		/// <summary>
		/// Determines whether one querystring contains every key=value pair that
		/// another querystring contains.
		/// </summary>
		/// <param name="superset">The querystring that should contain at least all the key=value pairs of the other.</param>
		/// <param name="subset">The querystring containing the set of key=value pairs to test for in the other.</param>
		/// <returns>
		/// 	<c>true</c> if <paramref name="superset"/> contains all the query parameters that <paramref name="subset"/> does; <c>false</c> otherwise.
		/// </returns>
		private static bool IsQuerySubsetOf(string superset, string subset) {
			NameValueCollection subsetArgs = HttpUtility.ParseQueryString(subset);
			NameValueCollection supersetArgs = HttpUtility.ParseQueryString(superset);
			return subsetArgs.Keys.Cast<string>().All(key => string.Equals(subsetArgs[key], supersetArgs[key], StringComparison.Ordinal));
		}

		/// <summary>
		/// Verifies that the openid.return_to field matches the URL of the actual HTTP request.
		/// </summary>
		/// <remarks>
		/// From OpenId Authentication 2.0 section 11.1:
		/// To verify that the "openid.return_to" URL matches the URL that is processing this assertion:
		///  * The URL scheme, authority, and path MUST be the same between the two URLs.
		///  * Any query parameters that are present in the "openid.return_to" URL MUST 
		///    also be present with the same values in the URL of the HTTP request the RP received.
		/// </remarks>
		private void VerifyReturnToMatchesRecipient() {
			ErrorUtilities.VerifyProtocol(
				string.Equals(this.Recipient.Scheme, this.ReturnTo.Scheme, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(this.Recipient.Authority, this.ReturnTo.Authority, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(this.Recipient.AbsolutePath, this.ReturnTo.AbsolutePath, StringComparison.Ordinal) &&
				IsQuerySubsetOf(this.Recipient.Query, this.ReturnTo.Query),
				OpenIdStrings.ReturnToParamDoesNotMatchRequestUrl,
				Protocol.openid.return_to,
				this.ReturnTo,
				this.Recipient);
		}
	}
}
