//-----------------------------------------------------------------------
// <copyright file="AuthenticationRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// Implements the <see cref="IAuthenticationRequest"/> interface
	/// so that OpenID Provider sites can easily respond to authentication
	/// requests.
	/// </summary>
	[Serializable]
	internal class AuthenticationRequest : HostProcessedRequest, IAuthenticationRequest {
		/// <summary>
		/// The positive assertion to send, if the host site chooses to send it.
		/// </summary>
		private readonly PositiveAssertionResponse positiveResponse;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationRequest"/> class.
		/// </summary>
		/// <param name="provider">The provider that received the request.</param>
		/// <param name="request">The incoming authentication request message.</param>
		internal AuthenticationRequest(OpenIdProvider provider, CheckIdRequest request)
			: base(provider, request) {
			Requires.NotNull(provider, "provider");

			this.positiveResponse = new PositiveAssertionResponse(request);

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

			Reporting.RecordEventOccurrence("AuthenticationRequest.IsDelegatedIdentifier", this.IsDelegatedIdentifier.ToString());
		}

		#region HostProcessedRequest members

		/// <summary>
		/// Gets or sets the provider endpoint.
		/// </summary>
		/// <value>
		/// The default value is the URL that the request came in on from the relying party.
		/// </value>
		public override Uri ProviderEndpoint {
			get { return this.positiveResponse.ProviderEndpoint; }
			set { this.positiveResponse.ProviderEndpoint = value; }
		}

		#endregion

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
					this.positiveResponse.LocalIdentifier = value;
				}

				this.positiveResponse.ClaimedIdentifier = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the provider has determined that the
		/// <see cref="ClaimedIdentifier"/> belongs to the currently logged in user
		/// and wishes to share this information with the consumer.
		/// </summary>
		public bool? IsAuthenticated { get; set; }

		#endregion

		/// <summary>
		/// Gets the original request message.
		/// </summary>
		protected new CheckIdRequest RequestMessage {
			get { return (CheckIdRequest)base.RequestMessage; }
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
			UriBuilder builder = new UriBuilder(this.ClaimedIdentifier);
			builder.Fragment = fragment;
			this.positiveResponse.ClaimedIdentifier = builder.Uri;
		}

		#endregion

		/// <summary>
		/// Sets the Claimed and Local identifiers even after they have been initially set.
		/// </summary>
		/// <param name="identifier">The value to set to the <see cref="ClaimedIdentifier"/> and <see cref="LocalIdentifier"/> properties.</param>
		internal void ResetClaimedAndLocalIdentifiers(Identifier identifier) {
			Requires.NotNull(identifier, "identifier");

			this.positiveResponse.ClaimedIdentifier = identifier;
			this.positiveResponse.LocalIdentifier = identifier;
		}

		/// <summary>
		/// Gets the response message, once <see cref="IsResponseReady" /> is <c>true</c>.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The response message.</returns>
		protected override async Task<IProtocolMessage> GetResponseMessageAsync(CancellationToken cancellationToken) {
			if (this.IsAuthenticated.HasValue) {
				return this.IsAuthenticated.Value ? (IProtocolMessage)this.positiveResponse : await this.GetNegativeResponseAsync();
			} else {
				return null;
			}
		}
	}
}
