//-----------------------------------------------------------------------
// <copyright file="OpenIdProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Offers services for a web page that is acting as an OpenID identity server.
	/// </summary>
	public sealed class OpenIdProvider : IDisposable {
		/// <summary>
		/// The name of the key to use in the HttpApplication cache to store the
		/// instance of <see cref="StandardProviderApplicationStore"/> to use.
		/// </summary>
		private const string ApplicationStoreKey = "DotNetOpenAuth.OpenId.Provider.OpenIdProvider.ApplicationStore";

		/// <summary>
		/// Backing field for the <see cref="SecuritySettings"/> property.
		/// </summary>
		private ProviderSecuritySettings securitySettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProvider"/> class.
		/// </summary>
		public OpenIdProvider()
			: this(DotNetOpenAuthSection.Configuration.OpenId.Provider.ApplicationStore.CreateInstance(HttpApplicationStore)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProvider"/> class.
		/// </summary>
		/// <param name="applicationStore">The application store to use.  Cannot be null.</param>
		public OpenIdProvider(IProviderApplicationStore applicationStore)
			: this(applicationStore, applicationStore) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProvider"/> class.
		/// </summary>
		/// <param name="associationStore">The association store to use.  Cannot be null.</param>
		/// <param name="nonceStore">The nonce store to use.  Cannot be null.</param>
		private OpenIdProvider(IAssociationStore<AssociationRelyingPartyType> associationStore, INonceStore nonceStore) {
			ErrorUtilities.VerifyArgumentNotNull(associationStore, "associationStore");
			ErrorUtilities.VerifyArgumentNotNull(nonceStore, "nonceStore");

			this.AssociationStore = associationStore;
			this.SecuritySettings = DotNetOpenAuthSection.Configuration.OpenId.Provider.SecuritySettings.CreateSecuritySettings();
			this.Channel = new OpenIdChannel(this.AssociationStore, nonceStore, this.SecuritySettings);
		}

		/// <summary>
		/// Gets the standard state storage mechanism that uses ASP.NET's
		/// HttpApplication state dictionary to store associations and nonces.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static IProviderApplicationStore HttpApplicationStore {
			get {
				HttpContext context = HttpContext.Current;
				ErrorUtilities.VerifyOperation(context != null, OpenIdStrings.StoreRequiredWhenNoHttpContextAvailable, typeof(IProviderApplicationStore).Name);
				var store = (IProviderApplicationStore)context.Application[ApplicationStoreKey];
				if (store == null) {
					context.Application.Lock();
					try {
						if ((store = (IProviderApplicationStore)context.Application[ApplicationStoreKey]) == null) {
							context.Application[ApplicationStoreKey] = store = new StandardProviderApplicationStore();
						}
					} finally {
						context.Application.UnLock();
					}
				}

				return store;
			}
		}

		/// <summary>
		/// Gets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel { get; internal set; }

		/// <summary>
		/// Gets the security settings used by this Provider.
		/// </summary>
		public ProviderSecuritySettings SecuritySettings {
			get {
				return this.securitySettings;
			}

			internal set {
				ErrorUtilities.VerifyArgumentNotNull(value, "value");
				this.securitySettings = value;
			}
		}

		/// <summary>
		/// Gets or sets the mechanism a host site can use to receive
		/// notifications of errors when communicating with remote parties.
		/// </summary>
		public IErrorReporting ErrorReporting { get; set; }

		/// <summary>
		/// Gets the association store.
		/// </summary>
		internal IAssociationStore<AssociationRelyingPartyType> AssociationStore { get; private set; }

		/// <summary>
		/// Gets the web request handler to use for discovery and the part of
		/// authentication where direct messages are sent to an untrusted remote party.
		/// </summary>
		internal IDirectWebRequestHandler WebRequestHandler {
			get { return this.Channel.WebRequestHandler; }
		}

		/// <summary>
		/// Gets the incoming OpenID request if there is one, or null if none was detected.
		/// </summary>
		/// <returns>The request that the hosting Provider should possibly process and then transmit the response for.</returns>
		/// <remarks>
		/// <para>Requests may be infrastructural to OpenID and allow auto-responses, or they may
		/// be authentication requests where the Provider site has to make decisions based
		/// on its own user database and policies.</para>
		/// <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		/// <exception cref="ProtocolException">Thrown if the incoming message is recognized but deviates from the protocol specification irrecoverably.</exception>
		public IRequest GetRequest() {
			return this.GetRequest(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Gets the incoming OpenID request if there is one, or null if none was detected.
		/// </summary>
		/// <param name="httpRequestInfo">The incoming HTTP request to extract the message from.</param>
		/// <returns>
		/// The request that the hosting Provider should process and then transmit the response for.
		/// Null if no valid OpenID request was detected in the given HTTP request.
		/// </returns>
		/// <remarks>
		/// Requests may be infrastructural to OpenID and allow auto-responses, or they may
		/// be authentication requests where the Provider site has to make decisions based
		/// on its own user database and policies.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the incoming message is recognized
		/// but deviates from the protocol specification irrecoverably.</exception>
		public IRequest GetRequest(HttpRequestInfo httpRequestInfo) {
			ErrorUtilities.VerifyArgumentNotNull(httpRequestInfo, "httpRequestInfo");
			IDirectedProtocolMessage incomingMessage = null;

			try {
				incomingMessage = this.Channel.ReadFromRequest(httpRequestInfo);
				if (incomingMessage == null) {
					// If the incoming request does not resemble an OpenID message at all,
					// it's probably a user who just navigated to this URL, and we should
					// just return null so the host can display a message to the user.
					if (httpRequestInfo.HttpMethod == "GET" && !httpRequestInfo.Url.QueryStringContainPrefixedParameters(Protocol.Default.openid.Prefix)) {
						return null;
					}

					ErrorUtilities.ThrowProtocol(MessagingStrings.UnexpectedMessageReceivedOfMany);
				}

				var checkIdMessage = incomingMessage as CheckIdRequest;
				if (checkIdMessage != null) {
					return new AuthenticationRequest(this, checkIdMessage);
				}

				var checkAuthMessage = incomingMessage as CheckAuthenticationRequest;
				if (checkAuthMessage != null) {
					return new AutoResponsiveRequest(this, incomingMessage, new CheckAuthenticationResponse(checkAuthMessage, this));
				}

				var associateMessage = incomingMessage as AssociateRequest;
				if (associateMessage != null) {
					return new AutoResponsiveRequest(this, incomingMessage, associateMessage.CreateResponse(this.AssociationStore, this.SecuritySettings));
				}

				throw ErrorUtilities.ThrowProtocol(MessagingStrings.UnexpectedMessageReceivedOfMany);
			} catch (ProtocolException ex) {
				IRequest errorResponse = this.GetErrorResponse(ex, httpRequestInfo, incomingMessage);
				if (errorResponse == null) {
					throw;
				}

				return errorResponse;
			}
		}

		/// <summary>
		/// Send an identity assertion on behalf of one of this Provider's
		/// members in order to redirect the user agent to a relying party
		/// web site and log him/her in immediately in one uninterrupted step.
		/// </summary>
		/// <param name="providerEndpoint">The absolute URL on the Provider site that receives OpenID messages.</param>
		/// <param name="relyingParty">The URL of the Relying Party web site.
		/// This will typically be the home page, but may be a longer URL if
		/// that Relying Party considers the scope of its realm to be more specific.
		/// The URL provided here must allow discovery of the Relying Party's
		/// XRDS document that advertises its OpenID RP endpoint.</param>
		/// <param name="claimedIdentifier">The Identifier you are asserting your member controls.</param>
		/// <param name="localIdentifier">The Identifier you know your user by internally.  This will typically
		/// be the same as <paramref name="claimedIdentifier"/>.</param>
		/// <param name="extensions">The extensions.</param>
		/// <returns>
		/// A <see cref="UserAgentResponse"/> object describing the HTTP response to send
		/// the user agent to allow the redirect with assertion to happen.
		/// </returns>
		public UserAgentResponse PrepareUnsolicitedAssertion(Uri providerEndpoint, Realm relyingParty, Identifier claimedIdentifier, Identifier localIdentifier, params IExtensionMessage[] extensions) {
			ErrorUtilities.VerifyArgumentNotNull(providerEndpoint, "providerEndpoint");
			ErrorUtilities.VerifyArgumentNotNull(relyingParty, "relyingParty");
			ErrorUtilities.VerifyArgumentNotNull(claimedIdentifier, "claimedIdentifier");
			ErrorUtilities.VerifyArgumentNotNull(localIdentifier, "localIdentifier");
			ErrorUtilities.VerifyArgumentNamed(providerEndpoint.IsAbsoluteUri, "providerEndpoint", OpenIdStrings.AbsoluteUriRequired);

			// Although the RP should do their due diligence to make sure that this OP
			// is authorized to send an assertion for the given claimed identifier,
			// do due diligence by performing our own discovery on the claimed identifier
			// and make sure that it is tied to this OP and OP local identifier.
			var serviceEndpoint = DotNetOpenAuth.OpenId.RelyingParty.ServiceEndpoint.CreateForClaimedIdentifier(claimedIdentifier, localIdentifier, new ProviderEndpointDescription(providerEndpoint, Protocol.Default.Version), null, null);
			var discoveredEndpoints = claimedIdentifier.Discover(this.WebRequestHandler);
			if (!discoveredEndpoints.Contains(serviceEndpoint)) {
				Logger.DebugFormat(
					"Failed to send unsolicited assertion for {0} because its discovered services did not include this endpoint.  This endpoint: {1}{2}  Discovered endpoints: {1}{3}",
					claimedIdentifier,
					Environment.NewLine,
					serviceEndpoint,
					discoveredEndpoints.ToStringDeferred(true));
				ErrorUtilities.ThrowProtocol(OpenIdStrings.UnsolicitedAssertionForUnrelatedClaimedIdentifier, claimedIdentifier);
			}

			Logger.InfoFormat("Preparing unsolicited assertion for {0}", claimedIdentifier);
			var returnToEndpoint = relyingParty.Discover(this.WebRequestHandler, true).FirstOrDefault();
			ErrorUtilities.VerifyProtocol(returnToEndpoint != null, OpenIdStrings.NoRelyingPartyEndpointDiscovered, relyingParty);

			var positiveAssertion = new PositiveAssertionResponse(returnToEndpoint) {
				ProviderEndpoint = providerEndpoint,
				ClaimedIdentifier = claimedIdentifier,
				LocalIdentifier = localIdentifier,
			};

			if (extensions != null) {
				foreach (IExtensionMessage extension in extensions) {
					positiveAssertion.Extensions.Add(extension);
				}
			}

			return this.Channel.PrepareResponse(positiveAssertion);
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing) {
			if (disposing) {
				// Tear off the instance member as a local variable for thread safety.
				IDisposable channel = this.Channel as IDisposable;
				if (channel != null) {
					channel.Dispose();
				}
			}
		}

		#endregion

		/// <summary>
		/// Prepares the return value for the GetRequest method in the event of an exception.
		/// </summary>
		/// <param name="ex">The exception that forms the basis of the error response.  Must not be null.</param>
		/// <param name="httpRequestInfo">The incoming HTTP request.  Must not be null.</param>
		/// <param name="incomingMessage">The incoming message.  May be null in the case that it was malformed.</param>
		/// <returns>
		/// Either the <see cref="IRequest"/> to return to the host site or null to indicate no response could be reasonably created and that the caller should rethrow the exception.
		/// </returns>
		private IRequest GetErrorResponse(ProtocolException ex, HttpRequestInfo httpRequestInfo, IDirectedProtocolMessage incomingMessage) {
			ErrorUtilities.VerifyArgumentNotNull(ex, "ex");
			ErrorUtilities.VerifyArgumentNotNull(httpRequestInfo, "httpRequestInfo");

			Logger.Error("An exception was generated while processing an incoming OpenID request.", ex);
			IErrorMessage errorMessage;

			// We must create the appropriate error message type (direct vs. indirect)
			// based on what we see in the request.
			if (httpRequestInfo.QueryString[Protocol.Default.openid.return_to] != null) {
				// An indirect request message from the RP
				// We need to return an indirect response error message so the RP can consume it.
				// Consistent with OpenID 2.0 section 5.2.3.
				var indirectRequest = incomingMessage as SignedResponseRequest;
				if (indirectRequest != null) {
					errorMessage = new IndirectErrorResponse(indirectRequest);
				} else {
					errorMessage = new IndirectErrorResponse(Protocol.Default.Version, new Uri(httpRequestInfo.QueryString[Protocol.Default.openid.return_to]));
				}
			} else if (httpRequestInfo.HttpMethod == "POST") {
				// A direct request message from the RP
				// We need to return a direct response error message so the RP can consume it.
				// Consistent with OpenID 2.0 section 5.1.2.2.
				errorMessage = new DirectErrorResponse(Protocol.Default.Version, incomingMessage);
			} else {
				// This may be an indirect request from an RP that was so badly
				// formed that we cannot even return an error to the RP.
				// The best we can do is display an error to the user.
				// Returning null cues the caller to "throw;"
				return null;
			}

			errorMessage.ErrorMessage = ex.GetAllMessages();

			// Allow host to log this error and issue a ticket #.
			// We tear off the field to a local var for thread safety.
			IErrorReporting hostErrorHandler = this.ErrorReporting;
			if (hostErrorHandler != null) {
				errorMessage.Contact = hostErrorHandler.Contact;
				errorMessage.Reference = hostErrorHandler.LogError(ex);
			}

			if (incomingMessage != null) {
				return new AutoResponsiveRequest(this, incomingMessage, errorMessage);
			} else {
				return new AutoResponsiveRequest(this, errorMessage);
			}
		}
	}
}
