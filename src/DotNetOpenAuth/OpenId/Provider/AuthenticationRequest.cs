//-----------------------------------------------------------------------
// <copyright file="AuthenticationRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Implements the <see cref="IAuthenticationRequest"/> interface
	/// so that OpenID Provider sites can easily respond to authentication
	/// requests.
	/// </summary>
	internal class AuthenticationRequest : Request, IAuthenticationRequest {
		/// <summary>
		/// The positive assertion to send, if the host site chooses to send it.
		/// </summary>
		private readonly PositiveAssertionResponse positiveResponse;

		/// <summary>
		/// The negative assertion to send, if the host site chooses to send it.
		/// </summary>
		private readonly NegativeAssertionResponse negativeResponse;

		/// <summary>
		/// A value indicating whether the host site has decided to assert the
		/// identity of the user agent operator.
		/// </summary>
		private bool? isAuthenticated;

		/// <summary>
		/// A value indicating whether the return_to URI on the RP was discoverable
		/// per the OpenID 2.0 specification.
		/// </summary>
		private bool? isReturnUrlDiscoverable;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationRequest"/> class.
		/// </summary>
		/// <param name="provider">The provider that received the request.</param>
		/// <param name="request">The incoming authentication request message.</param>
		internal AuthenticationRequest(OpenIdProvider provider, CheckIdRequest request)
			: base(provider, request) {
			this.positiveResponse = new PositiveAssertionResponse(request);
			this.negativeResponse = new NegativeAssertionResponse(request, provider.Channel);

			if (this.ClaimedIdentifier == Protocol.ClaimedIdentifierForOPIdentifier &&
				Protocol.ClaimedIdentifierForOPIdentifier != null) {
				// Force the hosting OP to deal with identifier_select by nulling out the two identifiers.
				this.IsDirectedIdentity = true;
				this.positiveResponse.ClaimedIdentifier = null;
				this.positiveResponse.LocalIdentifier = null;
			}

			// URL delegation is only detectable from 2.0 RPs, since openid.claimed_id isn't included from 1.0 RPs.
			// If the openid.claimed_id is present, and if it's different than the openid.identity argument, then
			// the RP has discovered a claimed identifier that has delegated authentication to this Provider.
			this.IsDelegatedIdentifier = this.ClaimedIdentifier != null && this.ClaimedIdentifier != this.LocalIdentifier;
		}

		/// <summary>
		/// Gets a value indicating whether the response is ready to be created and sent.
		/// </summary>
		public override bool IsResponseReady {
			get {
				// The null checks on the identifiers is to make sure that an identifier_select
				// has been resolved to actual identifiers.
				return this.IsAuthenticated.HasValue &&
					(!this.IsAuthenticated.Value || !this.IsDirectedIdentity || (this.LocalIdentifier != null && this.ClaimedIdentifier != null));
			}
		}

		#region IAuthenticationRequest Properties

		/// <summary>
		/// Gets the version of OpenID being used by the relying party that sent the request.
		/// </summary>
		public ProtocolVersion RelyingPartyVersion {
			get { return Protocol.Lookup(this.RequestMessage.Version).ProtocolVersion; }
		}

		/// <summary>
		/// Gets a value indicating whether the consumer demands an immediate response.
		/// If false, the consumer is willing to wait for the identity provider
		/// to authenticate the user.
		/// </summary>
		public bool Immediate {
			get { return this.RequestMessage.Immediate; }
		}

		/// <summary>
		/// Gets the URL the consumer site claims to use as its 'base' address.
		/// </summary>
		public Realm Realm {
			get { return this.RequestMessage.Realm; }
		}

		/// <summary>
		/// Gets a value indicating whether verification of the return URL claimed by the Relying Party
		/// succeeded.
		/// </summary>
		/// <remarks>
		/// Return URL verification is only attempted if this property is queried.
		/// The result of the verification is cached per request so calling this
		/// property getter multiple times in one request is not a performance hit.
		/// See OpenID Authentication 2.0 spec section 9.2.1.
		/// </remarks>
		public bool IsReturnUrlDiscoverable {
			get {
				ErrorUtilities.VerifyInternal(Realm != null, "Realm should have been read or derived by now.");
				if (!this.isReturnUrlDiscoverable.HasValue) {
					this.isReturnUrlDiscoverable = false; // assume not until we succeed
					try {
						foreach (var returnUrl in Realm.Discover(this.Provider.WebRequestHandler, false)) {
							Realm discoveredReturnToUrl = returnUrl.ReturnToEndpoint;

							// The spec requires that the return_to URLs given in an RPs XRDS doc
							// do not contain wildcards.
							if (discoveredReturnToUrl.DomainWildcard) {
								Logger.WarnFormat("Realm {0} contained return_to URL {1} which contains a wildcard, which is not allowed.", Realm, discoveredReturnToUrl);
								continue;
							}

							// Use the same rules as return_to/realm matching to check whether this
							// URL fits the return_to URL we were given.
							if (discoveredReturnToUrl.Contains(this.RequestMessage.ReturnTo)) {
								this.isReturnUrlDiscoverable = true;
								break; // no need to keep looking after we find a match
							}
						}
					} catch (ProtocolException ex) {
						// Don't do anything else.  We quietly fail at return_to verification and return false.
						Logger.InfoFormat("Relying party discovery at URL {0} failed.  {1}", Realm, ex);
					} catch (WebException ex) {
						// Don't do anything else.  We quietly fail at return_to verification and return false.
						Logger.InfoFormat("Relying party discovery at URL {0} failed.  {1}", Realm, ex);
					}
				}

				return this.isReturnUrlDiscoverable.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the Provider should help the user
		/// select a Claimed Identifier to send back to the relying party.
		/// </summary>
		public bool IsDirectedIdentity { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the requesting Relying Party is using a delegated URL.
		/// </summary>
		/// <remarks>
		/// When delegated identifiers are used, the <see cref="ClaimedIdentifier"/> should not
		/// be changed at the Provider during authentication.
		/// Delegation is only detectable on requests originating from OpenID 2.0 relying parties.
		/// A relying party implementing only OpenID 1.x may use delegation and this property will
		/// return false anyway.
		/// </remarks>
		public bool IsDelegatedIdentifier { get; private set; }

		/// <summary>
		/// Gets or sets the Local Identifier to this OpenID Provider of the user attempting
		/// to authenticate.  Check <see cref="IsDirectedIdentity"/> to see if
		/// this value is valid.
		/// </summary>
		/// <remarks>
		/// This may or may not be the same as the Claimed Identifier that the user agent
		/// originally supplied to the relying party.  The Claimed Identifier
		/// endpoint may be delegating authentication to this provider using
		/// this provider's local id, which is what this property contains.
		/// Use this identifier when looking up this user in the provider's user account
		/// list.
		/// </remarks>
		public Identifier LocalIdentifier {
			get {
				return this.positiveResponse.LocalIdentifier;
			}

			set {
				// Keep LocalIdentifier and ClaimedIdentifier in sync for directed identity.
				if (this.IsDirectedIdentity) {
					if (this.ClaimedIdentifier != null && this.ClaimedIdentifier != value) {
						throw new InvalidOperationException(OpenIdStrings.IdentifierSelectRequiresMatchingIdentifiers);
					}

					this.positiveResponse.ClaimedIdentifier = value;
				}

				this.positiveResponse.LocalIdentifier = value;
				this.ResetUserAgentResponse();
			}
		}

		/// <summary>
		/// Gets or sets the identifier that the user agent is claiming at the relying party site.
		/// Check <see cref="IsDirectedIdentity"/> to see if this value is valid.
		/// </summary>
		/// <remarks>
		/// 	<para>This property can only be set if <see cref="IsDelegatedIdentifier"/> is
		/// false, to prevent breaking URL delegation.</para>
		/// 	<para>This will not be the same as this provider's local identifier for the user
		/// if the user has set up his/her own identity page that points to this
		/// provider for authentication.</para>
		/// 	<para>The provider may use this identifier for displaying to the user when
		/// asking for the user's permission to authenticate to the relying party.</para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown from the setter
		/// if <see cref="IsDelegatedIdentifier"/> is true.</exception>
		public Identifier ClaimedIdentifier {
			get {
				return this.positiveResponse.ClaimedIdentifier;
			}

			set {
				// Keep LocalIdentifier and ClaimedIdentifier in sync for directed identity.
				if (this.IsDirectedIdentity) {
					ErrorUtilities.VerifyOperation(!(this.LocalIdentifier != null && this.LocalIdentifier != value), OpenIdStrings.IdentifierSelectRequiresMatchingIdentifiers);
					this.positiveResponse.LocalIdentifier = value;
				}

				ErrorUtilities.VerifyOperation(!this.IsDelegatedIdentifier, OpenIdStrings.ClaimedIdentifierCannotBeSetOnDelegatedAuthentication);
				this.positiveResponse.ClaimedIdentifier = value;
				this.ResetUserAgentResponse();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the provider has determined that the
		/// <see cref="ClaimedIdentifier"/> belongs to the currently logged in user
		/// and wishes to share this information with the consumer.
		/// </summary>
		public bool? IsAuthenticated {
			get {
				return this.isAuthenticated;
			}

			set {
				this.isAuthenticated = value;
				this.ResetUserAgentResponse();
			}
		}

		#endregion

		/// <summary>
		/// Gets the original request message.
		/// </summary>
		protected new CheckIdRequest RequestMessage {
			get { return (CheckIdRequest)base.RequestMessage; }
		}

		/// <summary>
		/// Gets the response message, once <see cref="IsResponseReady"/> is <c>true</c>.
		/// </summary>
		protected override IProtocolMessage ResponseMessage {
			get {
				if (this.isAuthenticated.HasValue) {
					return this.isAuthenticated.Value ? (IProtocolMessage)this.positiveResponse : this.negativeResponse;
				} else {
					return null;
				}
			}
		}

		#region IAuthenticationRequest Methods

		/// <summary>
		/// Adds an optional fragment (#fragment) portion to the ClaimedIdentifier.
		/// Useful for identifier recycling.
		/// </summary>
		/// <param name="fragment">Should not include the # prefix character as that will be added internally.
		/// May be null or the empty string to clear a previously set fragment.</param>
		/// <remarks>
		/// 	<para>Unlike the <see cref="ClaimedIdentifier"/> property, which can only be set if
		/// using directed identity, this method can be called on any URI claimed identifier.</para>
		/// 	<para>Because XRI claimed identifiers (the canonical IDs) are never recycled,
		/// this method should<i>not</i> be called for XRIs.</para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// Thrown when this method is called on an XRI, or on a directed identity
		/// request before the <see cref="ClaimedIdentifier"/> property is set.
		/// </exception>
		public void SetClaimedIdentifierFragment(string fragment) {
			ErrorUtilities.VerifyOperation(!(this.IsDirectedIdentity && this.ClaimedIdentifier == null), OpenIdStrings.ClaimedIdentifierMustBeSetFirst);
			ErrorUtilities.VerifyOperation(!(this.ClaimedIdentifier is XriIdentifier), OpenIdStrings.FragmentNotAllowedOnXRIs);

			UriBuilder builder = new UriBuilder(this.ClaimedIdentifier);
			builder.Fragment = fragment;
			this.positiveResponse.ClaimedIdentifier = builder.Uri;
		}

		#endregion
	}
}
