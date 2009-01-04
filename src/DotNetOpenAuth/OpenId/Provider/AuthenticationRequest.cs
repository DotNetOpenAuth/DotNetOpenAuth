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
			this.negativeResponse = new NegativeAssertionResponse(request);

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

		public override bool IsResponseReady {
			get {
				// The null checks on the identifiers is to make sure that an identifier_select
				// has been resolved to actual identifiers.
				return this.IsAuthenticated.HasValue &&
					(!this.IsAuthenticated.Value || !this.IsDirectedIdentity || (this.LocalIdentifier != null && this.ClaimedIdentifier != null));
			}
		}

		#region IAuthenticationRequest Properties

		public ProtocolVersion RelyingPartyVersion {
			get { return Protocol.Lookup(this.RequestMessage.Version).ProtocolVersion; }
		}

		public bool Immediate {
			get { return this.RequestMessage.Immediate; }
		}

		public Realm Realm {
			get { return this.RequestMessage.Realm; }
		}

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

		public bool IsDirectedIdentity { get; private set; }

		public bool IsDelegatedIdentifier { get; private set; }

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

		protected new CheckIdRequest RequestMessage {
			get { return (CheckIdRequest)base.RequestMessage; }
		}

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
