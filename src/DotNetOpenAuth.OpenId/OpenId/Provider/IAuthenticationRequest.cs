//-----------------------------------------------------------------------
// <copyright file="IAuthenticationRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Instances of this interface represent incoming authentication requests.
	/// This interface provides the details of the request and allows setting
	/// the response.
	/// </summary>
	[ContractClass(typeof(IAuthenticationRequestContract))]
	public interface IAuthenticationRequest : IHostProcessedRequest {
		/// <summary>
		/// Gets a value indicating whether the Provider should help the user 
		/// select a Claimed Identifier to send back to the relying party.
		/// </summary>
		bool IsDirectedIdentity { get; }

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
		bool IsDelegatedIdentifier { get; }

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
		Identifier LocalIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the identifier that the user agent is claiming at the relying party site.
		/// Check <see cref="IsDirectedIdentity"/> to see if this value is valid.
		/// </summary>
		/// <remarks>
		/// <para>This property can only be set if <see cref="IsDelegatedIdentifier"/> is
		/// false, to prevent breaking URL delegation.</para>
		/// <para>This will not be the same as this provider's local identifier for the user
		/// if the user has set up his/her own identity page that points to this 
		/// provider for authentication.</para>
		/// <para>The provider may use this identifier for displaying to the user when
		/// asking for the user's permission to authenticate to the relying party.</para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown from the setter 
		/// if <see cref="IsDelegatedIdentifier"/> is true.</exception>
		Identifier ClaimedIdentifier { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the provider has determined that the 
		/// <see cref="ClaimedIdentifier"/> belongs to the currently logged in user
		/// and wishes to share this information with the consumer.
		/// </summary>
		bool? IsAuthenticated { get; set; }

		/// <summary>
		/// Adds an optional fragment (#fragment) portion to the ClaimedIdentifier.
		/// Useful for identifier recycling.
		/// </summary>
		/// <param name="fragment">
		/// Should not include the # prefix character as that will be added internally.
		/// May be null or the empty string to clear a previously set fragment.
		/// </param>
		/// <remarks>
		/// <para>Unlike the <see cref="ClaimedIdentifier"/> property, which can only be set if
		/// using directed identity, this method can be called on any URI claimed identifier.</para>
		/// <para>Because XRI claimed identifiers (the canonical IDs) are never recycled,
		/// this method should<i>not</i> be called for XRIs.</para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// Thrown when this method is called on an XRI, or on a directed identity 
		/// request before the <see cref="ClaimedIdentifier"/> property is set.
		/// </exception>
		void SetClaimedIdentifierFragment(string fragment);
	}

	/// <summary>
	/// Code contract class for the <see cref="IAuthenticationRequest"/> type.
	/// </summary>
	[ContractClassFor(typeof(IAuthenticationRequest))]
	internal abstract class IAuthenticationRequestContract : IAuthenticationRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="IAuthenticationRequestContract"/> class.
		/// </summary>
		protected IAuthenticationRequestContract() {
		}

		#region IAuthenticationRequest Properties

		/// <summary>
		/// Gets a value indicating whether the Provider should help the user
		/// select a Claimed Identifier to send back to the relying party.
		/// </summary>
		bool IAuthenticationRequest.IsDirectedIdentity {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets a value indicating whether the requesting Relying Party is using a delegated URL.
		/// </summary>
		/// <remarks>
		/// When delegated identifiers are used, the <see cref="IAuthenticationRequest.ClaimedIdentifier"/> should not
		/// be changed at the Provider during authentication.
		/// Delegation is only detectable on requests originating from OpenID 2.0 relying parties.
		/// A relying party implementing only OpenID 1.x may use delegation and this property will
		/// return false anyway.
		/// </remarks>
		bool IAuthenticationRequest.IsDelegatedIdentifier {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets or sets the Local Identifier to this OpenID Provider of the user attempting
		/// to authenticate.  Check <see cref="IAuthenticationRequest.IsDirectedIdentity"/> to see if
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
		Identifier IAuthenticationRequest.LocalIdentifier {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets or sets the identifier that the user agent is claiming at the relying party site.
		/// Check <see cref="IAuthenticationRequest.IsDirectedIdentity"/> to see if this value is valid.
		/// </summary>
		/// <remarks>
		/// 	<para>This property can only be set if <see cref="IAuthenticationRequest.IsDelegatedIdentifier"/> is
		/// false, to prevent breaking URL delegation.</para>
		/// 	<para>This will not be the same as this provider's local identifier for the user
		/// if the user has set up his/her own identity page that points to this
		/// provider for authentication.</para>
		/// 	<para>The provider may use this identifier for displaying to the user when
		/// asking for the user's permission to authenticate to the relying party.</para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown from the setter
		/// if <see cref="IAuthenticationRequest.IsDelegatedIdentifier"/> is true.</exception>
		Identifier IAuthenticationRequest.ClaimedIdentifier {
			get {
				throw new NotImplementedException();
			}

			set {
				IAuthenticationRequest req = this;
				Requires.ValidState(!req.IsDelegatedIdentifier, OpenIdStrings.ClaimedIdentifierCannotBeSetOnDelegatedAuthentication);
				Requires.ValidState(!req.IsDirectedIdentity || !(req.LocalIdentifier != null && req.LocalIdentifier != value), OpenIdStrings.IdentifierSelectRequiresMatchingIdentifiers);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the provider has determined that the
		/// <see cref="IAuthenticationRequest.ClaimedIdentifier"/> belongs to the currently logged in user
		/// and wishes to share this information with the consumer.
		/// </summary>
		bool? IAuthenticationRequest.IsAuthenticated {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		#endregion

		#region IHostProcessedRequest Properties

		/// <summary>
		/// Gets the version of OpenID being used by the relying party that sent the request.
		/// </summary>
		ProtocolVersion IHostProcessedRequest.RelyingPartyVersion {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets the URL the consumer site claims to use as its 'base' address.
		/// </summary>
		Realm IHostProcessedRequest.Realm {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets a value indicating whether the consumer demands an immediate response.
		/// If false, the consumer is willing to wait for the identity provider
		/// to authenticate the user.
		/// </summary>
		bool IHostProcessedRequest.Immediate {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets or sets the provider endpoint claimed in the positive assertion.
		/// </summary>
		/// <value>
		/// The default value is the URL that the request came in on from the relying party.
		/// This value MUST match the value for the OP Endpoint in the discovery results for the
		/// claimed identifier being asserted in a positive response.
		/// </value>
		Uri IHostProcessedRequest.ProviderEndpoint {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		#endregion

		#region IRequest Properties

		/// <summary>
		/// Gets a value indicating whether the response is ready to be sent to the user agent.
		/// </summary>
		/// <remarks>
		/// This property returns false if there are properties that must be set on this
		/// request instance before the response can be sent.
		/// </remarks>
		bool IRequest.IsResponseReady {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets or sets the security settings that apply to this request.
		/// </summary>
		/// <value>
		/// Defaults to the OpenIdProvider.SecuritySettings on the OpenIdProvider.
		/// </value>
		ProviderSecuritySettings IRequest.SecuritySettings {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		#endregion

		#region IAuthenticationRequest Methods

		/// <summary>
		/// Adds an optional fragment (#fragment) portion to the ClaimedIdentifier.
		/// Useful for identifier recycling.
		/// </summary>
		/// <param name="fragment">Should not include the # prefix character as that will be added internally.
		/// May be null or the empty string to clear a previously set fragment.</param>
		/// <remarks>
		/// 	<para>Unlike the <see cref="IAuthenticationRequest.ClaimedIdentifier"/> property, which can only be set if
		/// using directed identity, this method can be called on any URI claimed identifier.</para>
		/// 	<para>Because XRI claimed identifiers (the canonical IDs) are never recycled,
		/// this method should<i>not</i> be called for XRIs.</para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// Thrown when this method is called on an XRI, or on a directed identity
		/// request before the <see cref="IAuthenticationRequest.ClaimedIdentifier"/> property is set.
		/// </exception>
		void IAuthenticationRequest.SetClaimedIdentifierFragment(string fragment) {
			Requires.ValidState(!(((IAuthenticationRequest)this).IsDirectedIdentity && ((IAuthenticationRequest)this).ClaimedIdentifier == null), OpenIdStrings.ClaimedIdentifierMustBeSetFirst);
			Requires.ValidState(!(((IAuthenticationRequest)this).ClaimedIdentifier is XriIdentifier), OpenIdStrings.FragmentNotAllowedOnXRIs);

			throw new NotImplementedException();
		}

		#endregion

		#region IHostProcessedRequest Methods

		/// <summary>
		/// Attempts to perform relying party discovery of the return URL claimed by the Relying Party.
		/// </summary>
		/// <param name="webRequestHandler">The web request handler to use for the RP discovery request.</param>
		/// <returns>
		/// The details of how successful the relying party discovery was.
		/// </returns>
		/// <remarks>
		/// 	<para>Return URL verification is only attempted if this method is called.</para>
		/// 	<para>See OpenID Authentication 2.0 spec section 9.2.1.</para>
		/// </remarks>
		RelyingPartyDiscoveryResult IHostProcessedRequest.IsReturnUrlDiscoverable(IDirectWebRequestHandler webRequestHandler) {
			throw new NotImplementedException();
		}

		#endregion

		#region IRequest Methods

		/// <summary>
		/// Adds an extension to the response to send to the relying party.
		/// </summary>
		/// <param name="extension">The extension to add to the response message.</param>
		void IRequest.AddResponseExtension(DotNetOpenAuth.OpenId.Messages.IOpenIdMessageExtension extension) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removes any response extensions previously added using <see cref="IRequest.AddResponseExtension"/>.
		/// </summary>
		/// <remarks>
		/// This should be called before sending a negative response back to the relying party
		/// if extensions were already added, since negative responses cannot carry extensions.
		/// </remarks>
		void IRequest.ClearResponseExtensions() {
		}

		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <typeparam name="T">The type of the extension.</typeparam>
		/// <returns>
		/// An instance of the extension initialized with values passed in with the request.
		/// </returns>
		T IRequest.GetExtension<T>() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <param name="extensionType">The type of the extension.</param>
		/// <returns>
		/// An instance of the extension initialized with values passed in with the request.
		/// </returns>
		DotNetOpenAuth.OpenId.Messages.IOpenIdMessageExtension IRequest.GetExtension(Type extensionType) {
			throw new NotImplementedException();
		}

		#endregion
	}
}
