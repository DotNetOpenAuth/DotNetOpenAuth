//-----------------------------------------------------------------------
// <copyright file="HostProcessedRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Runtime.Serialization;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// A base class from which identity and non-identity RP requests can derive.
	/// </summary>
	[Serializable]
	internal abstract class HostProcessedRequest : Request, IHostProcessedRequest, IDeserializationCallback {
		/// <summary>
		/// The negative assertion to send, if the host site chooses to send it.
		/// </summary>
		[NonSerialized]
		private Lazy<Task<NegativeAssertionResponse>> negativeResponse;

		/// <summary>
		/// A cache of the result from discovery of the Realm URL.
		/// </summary>
		private RelyingPartyDiscoveryResult? realmDiscoveryResult;

		/// <summary>
		/// Initializes a new instance of the <see cref="HostProcessedRequest"/> class.
		/// </summary>
		/// <param name="provider">The provider that received the request.</param>
		/// <param name="request">The incoming request message.</param>
		protected HostProcessedRequest(OpenIdProvider provider, SignedResponseRequest request)
			: base(request, provider.SecuritySettings) {
			Requires.NotNull(provider, "provider");

			this.SharedInitialization(provider);
			Reporting.RecordEventOccurrence(this, request.Realm);
		}

		#region IHostProcessedRequest Properties

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
		/// Gets or sets the provider endpoint.
		/// </summary>
		/// <value>
		/// The default value is the URL that the request came in on from the relying party.
		/// </value>
		public abstract Uri ProviderEndpoint { get; set; }

		#endregion

		/// <summary>
		/// Gets a value indicating whether realm discovery been performed.
		/// </summary>
		internal bool HasRealmDiscoveryBeenPerformed {
			get { return this.realmDiscoveryResult.HasValue; }
		}

		/// <summary>
		/// Gets the original request message.
		/// </summary>
		/// <value>This may be null in the case of an unrecognizable message.</value>
		protected new SignedResponseRequest RequestMessage {
			get { return (SignedResponseRequest)base.RequestMessage; }
		}

		#region IHostProcessedRequest Methods

		/// <summary>
		/// Gets a value indicating whether verification of the return URL claimed by the Relying Party
		/// succeeded.
		/// </summary>
		/// <param name="hostFactories">The host factories.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// Result of realm discovery.
		/// </returns>
		/// <remarks>
		/// Return URL verification is only attempted if this property is queried.
		/// The result of the verification is cached per request so calling this
		/// property getter multiple times in one request is not a performance hit.
		/// See OpenID Authentication 2.0 spec section 9.2.1.
		/// </remarks>
		public async Task<RelyingPartyDiscoveryResult> IsReturnUrlDiscoverableAsync(IHostFactories hostFactories, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(hostFactories, "hostFactories");
			if (!this.realmDiscoveryResult.HasValue) {
				this.realmDiscoveryResult = await this.IsReturnUrlDiscoverableCoreAsync(hostFactories, cancellationToken);
			}

			return this.realmDiscoveryResult.Value;
		}

		#endregion

		/// <summary>
		/// Runs when the entire object graph has been deserialized.
		/// </summary>
		/// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
		void IDeserializationCallback.OnDeserialization(object sender) {
			// Bug: user_setup_url won't be created for OpenID 1.1 RPs in this path.
			this.SharedInitialization(null);
		}

		/// <summary>
		/// Gets the negative response.
		/// </summary>
		/// <returns>The negative assertion message.</returns>
		protected Task<NegativeAssertionResponse> GetNegativeResponseAsync() {
			return this.negativeResponse.Value;
		}

		/// <summary>
		/// Gets a value indicating whether verification of the return URL claimed by the Relying Party
		/// succeeded.
		/// </summary>
		/// <param name="hostFactories">The host factories.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// Result of realm discovery.
		/// </returns>
		private async Task<RelyingPartyDiscoveryResult> IsReturnUrlDiscoverableCoreAsync(IHostFactories hostFactories, CancellationToken cancellationToken) {
			Requires.NotNull(hostFactories, "hostFactories");
			ErrorUtilities.VerifyInternal(this.Realm != null, "Realm should have been read or derived by now.");

			try {
				if (this.SecuritySettings.RequireSsl && this.Realm.Scheme != Uri.UriSchemeHttps) {
					Logger.OpenId.WarnFormat("RP discovery failed because RequireSsl is true and RP discovery would begin at insecure URL {0}.", this.Realm);
					return RelyingPartyDiscoveryResult.NoServiceDocument;
				}

				var returnToEndpoints = await this.Realm.DiscoverReturnToEndpointsAsync(hostFactories, false, cancellationToken);
				if (returnToEndpoints == null) {
					return RelyingPartyDiscoveryResult.NoServiceDocument;
				}

				foreach (var returnUrl in returnToEndpoints) {
					Realm discoveredReturnToUrl = returnUrl.ReturnToEndpoint;

					// The spec requires that the return_to URLs given in an RPs XRDS doc
					// do not contain wildcards.
					if (discoveredReturnToUrl.DomainWildcard) {
						Logger.Yadis.WarnFormat("Realm {0} contained return_to URL {1} which contains a wildcard, which is not allowed.", Realm, discoveredReturnToUrl);
						continue;
					}

					// Use the same rules as return_to/realm matching to check whether this
					// URL fits the return_to URL we were given.
					if (discoveredReturnToUrl.Contains(this.RequestMessage.ReturnTo)) {
						// no need to keep looking after we find a match
						return RelyingPartyDiscoveryResult.Success;
					}
				}
			} catch (ProtocolException ex) {
				// Don't do anything else.  We quietly fail at return_to verification and return false.
				Logger.Yadis.InfoFormat("Relying party discovery at URL {0} failed.  {1}", Realm, ex);
				return RelyingPartyDiscoveryResult.NoServiceDocument;
			} catch (WebException ex) {
				// Don't do anything else.  We quietly fail at return_to verification and return false.
				Logger.Yadis.InfoFormat("Relying party discovery at URL {0} failed.  {1}", Realm, ex);
				return RelyingPartyDiscoveryResult.NoServiceDocument;
			}

			return RelyingPartyDiscoveryResult.NoMatchingReturnTo;
		}

		/// <summary>
		/// Performs initialization common to construction and deserialization.
		/// </summary>
		/// <param name="provider">The provider.</param>
		private void SharedInitialization(OpenIdProvider provider) {
			this.negativeResponse = new Lazy<Task<NegativeAssertionResponse>>(() => NegativeAssertionResponse.CreateAsync(this.RequestMessage, CancellationToken.None, provider.Channel));
		}
	}
}
