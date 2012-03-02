//-----------------------------------------------------------------------
// <copyright file="IHostProcessedRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Interface exposing incoming messages to the OpenID Provider that
	/// require interaction with the host site.
	/// </summary>
	[ContractClass(typeof(IHostProcessedRequestContract))]
	public interface IHostProcessedRequest : IRequest {
		/// <summary>
		/// Gets the version of OpenID being used by the relying party that sent the request.
		/// </summary>
		ProtocolVersion RelyingPartyVersion { get; }

		/// <summary>
		/// Gets the URL the consumer site claims to use as its 'base' address.
		/// </summary>
		Realm Realm { get; }

		/// <summary>
		/// Gets a value indicating whether the consumer demands an immediate response.
		/// If false, the consumer is willing to wait for the identity provider
		/// to authenticate the user.
		/// </summary>
		bool Immediate { get; }

		/// <summary>
		/// Gets or sets the provider endpoint claimed in the positive assertion.
		/// </summary>
		/// <value>
		/// The default value is the URL that the request came in on from the relying party.
		/// This value MUST match the value for the OP Endpoint in the discovery results for the
		/// claimed identifier being asserted in a positive response.
		/// </value>
		Uri ProviderEndpoint { get; set; }

		/// <summary>
		/// Attempts to perform relying party discovery of the return URL claimed by the Relying Party.
		/// </summary>
		/// <param name="webRequestHandler">The web request handler.</param>
		/// <returns>
		/// The details of how successful the relying party discovery was.
		/// </returns>
		/// <remarks>
		///   <para>Return URL verification is only attempted if this method is called.</para>
		///   <para>See OpenID Authentication 2.0 spec section 9.2.1.</para>
		/// </remarks>
		RelyingPartyDiscoveryResult IsReturnUrlDiscoverable(IDirectWebRequestHandler webRequestHandler);
	}

	/// <summary>
	/// Code contract for the <see cref="IHostProcessedRequest"/> type.
	/// </summary>
	[ContractClassFor(typeof(IHostProcessedRequest))]
	internal abstract class IHostProcessedRequestContract : IHostProcessedRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="IHostProcessedRequestContract"/> class.
		/// </summary>
		protected IHostProcessedRequestContract() {
		}

		#region IHostProcessedRequest Properties

		/// <summary>
		/// Gets the version of OpenID being used by the relying party that sent the request.
		/// </summary>
		ProtocolVersion IHostProcessedRequest.RelyingPartyVersion {
			get { throw new System.NotImplementedException(); }
		}

		/// <summary>
		/// Gets the URL the consumer site claims to use as its 'base' address.
		/// </summary>
		Realm IHostProcessedRequest.Realm {
			get { throw new System.NotImplementedException(); }
		}

		/// <summary>
		/// Gets a value indicating whether the consumer demands an immediate response.
		/// If false, the consumer is willing to wait for the identity provider
		/// to authenticate the user.
		/// </summary>
		bool IHostProcessedRequest.Immediate {
			get { throw new System.NotImplementedException(); }
		}

		/// <summary>
		/// Gets or sets the provider endpoint.
		/// </summary>
		/// <value>
		/// The default value is the URL that the request came in on from the relying party.
		/// </value>
		Uri IHostProcessedRequest.ProviderEndpoint {
			get {
				Contract.Ensures(Contract.Result<Uri>() != null);
				throw new NotImplementedException();
			}

			set {
				Contract.Requires(value != null);
				throw new NotImplementedException();
			}
		}

		#endregion

		#region IRequest Members

		/// <summary>
		/// Gets or sets the security settings that apply to this request.
		/// </summary>
		/// <value>
		/// Defaults to the OpenIdProvider.SecuritySettings on the OpenIdProvider.
		/// </value>
		ProviderSecuritySettings IRequest.SecuritySettings {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets a value indicating whether the response is ready to be sent to the user agent.
		/// </summary>
		/// <remarks>
		/// This property returns false if there are properties that must be set on this
		/// request instance before the response can be sent.
		/// </remarks>
		bool IRequest.IsResponseReady {
			get { throw new System.NotImplementedException(); }
		}

		/// <summary>
		/// Adds an extension to the response to send to the relying party.
		/// </summary>
		/// <param name="extension">The extension to add to the response message.</param>
		void IRequest.AddResponseExtension(DotNetOpenAuth.OpenId.Messages.IOpenIdMessageExtension extension) {
			throw new System.NotImplementedException();
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
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <param name="extensionType">The type of the extension.</param>
		/// <returns>
		/// An instance of the extension initialized with values passed in with the request.
		/// </returns>
		DotNetOpenAuth.OpenId.Messages.IOpenIdMessageExtension IRequest.GetExtension(System.Type extensionType) {
			throw new System.NotImplementedException();
		}

		#endregion

		#region IHostProcessedRequest Methods

		/// <summary>
		/// Attempts to perform relying party discovery of the return URL claimed by the Relying Party.
		/// </summary>
		/// <param name="webRequestHandler">The web request handler.</param>
		/// <returns>
		/// The details of how successful the relying party discovery was.
		/// </returns>
		/// <remarks>
		///   <para>Return URL verification is only attempted if this method is called.</para>
		///   <para>See OpenID Authentication 2.0 spec section 9.2.1.</para>
		/// </remarks>
		RelyingPartyDiscoveryResult IHostProcessedRequest.IsReturnUrlDiscoverable(IDirectWebRequestHandler webRequestHandler) {
			Requires.NotNull(webRequestHandler, "webRequestHandler");
			throw new System.NotImplementedException();
		}

		#endregion
	}
}
