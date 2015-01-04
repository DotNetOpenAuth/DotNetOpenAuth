//-----------------------------------------------------------------------
// <copyright file="OAuth2AuthorizationServerChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Net.Mime;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.AuthServer.Messages;
	using DotNetOpenAuth.OAuth2.Messages;
	using Validation;

	/// <summary>
	/// The channel for the OAuth protocol.
	/// </summary>
	internal class OAuth2AuthorizationServerChannel : OAuth2ChannelBase, IOAuth2ChannelWithAuthorizationServer {
		/// <summary>
		/// The messages receivable by this channel.
		/// </summary>
		private static readonly Type[] MessageTypes = new Type[] {
			typeof(AccessTokenRefreshRequestAS),
			typeof(AccessTokenAuthorizationCodeRequestAS),
			typeof(AccessTokenResourceOwnerPasswordCredentialsRequest),
			typeof(AccessTokenClientCredentialsRequest),
			typeof(EndUserAuthorizationRequest),
			typeof(EndUserAuthorizationImplicitRequest),
			typeof(EndUserAuthorizationFailedResponse),
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2AuthorizationServerChannel"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="clientAuthenticationModule">The aggregating client authentication module.</param>
		protected internal OAuth2AuthorizationServerChannel(IAuthorizationServerHost authorizationServer, ClientAuthenticationModule clientAuthenticationModule)
			: base(MessageTypes, InitializeBindingElements(authorizationServer, clientAuthenticationModule)) {
			Requires.NotNull(authorizationServer, "authorizationServer");
			this.AuthorizationServer = authorizationServer;
		}

		/// <summary>
		/// Gets the authorization server.
		/// </summary>
		/// <value>The authorization server.</value>
		public IAuthorizationServerHost AuthorizationServer { get; private set; }

		/// <summary>
		/// Gets or sets the service that checks whether a granted set of scopes satisfies a required set of scopes.
		/// </summary>
		public IScopeSatisfiedCheck ScopeSatisfiedCheck { get; set; }

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response.
		/// </summary>
		/// <param name="response">The response that is anticipated to contain an protocol message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The deserialized message parts, if found.  Null otherwise.
		/// </returns>
		/// <exception cref="System.NotImplementedException">Always thrown.</exception>
		/// <exception cref="ProtocolException">Thrown when the response is not valid.</exception>
		protected override Task<IDictionary<string, string>> ReadFromResponseCoreAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Queues a message for sending in the response stream.
		/// </summary>
		/// <param name="response">The message to send as a response.</param>
		/// <returns>
		/// The pending user agent redirect based message to be sent as an HttpResponse.
		/// </returns>
		/// <remarks>
		/// This method implements spec OAuth V1.0 section 5.3.
		/// </remarks>
		protected override HttpResponseMessage PrepareDirectResponse(IProtocolMessage response) {
			var webResponse = new HttpResponseMessage();
			ApplyMessageTemplate(response, webResponse);
			string json = this.SerializeAsJson(response);
			webResponse.Content = new StringContent(json, Encoding.UTF8, JsonEncoded);
			return webResponse;
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="request">The request to search for an embedded message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The deserialized message, if one is found.  Null otherwise.
		/// </returns>
		protected override async Task<IDirectedProtocolMessage> ReadFromRequestCoreAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			Requires.NotNull(request, "request");

			if (!string.IsNullOrEmpty(request.RequestUri.Fragment)) {
				var fields = HttpUtility.ParseQueryString(request.RequestUri.Fragment.Substring(1)).ToDictionary();

				MessageReceivingEndpoint recipient;
				try {
					recipient = request.GetRecipient();
				} catch (ArgumentException ex) {
					Logger.Messaging.WarnFormat("Unrecognized HTTP request: " + ex.ToString());
					return null;
				}

				return (IDirectedProtocolMessage)this.Receive(fields, recipient);
			}

			return await base.ReadFromRequestCoreAsync(request, cancellationToken);
		}

		/// <summary>
		/// Initializes the binding elements for the OAuth channel.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="clientAuthenticationModule">The aggregating client authentication module.</param>
		/// <returns>
		/// An array of binding elements used to initialize the channel.
		/// </returns>
		private static IChannelBindingElement[] InitializeBindingElements(IAuthorizationServerHost authorizationServer, ClientAuthenticationModule clientAuthenticationModule) {
			Requires.NotNull(authorizationServer, "authorizationServer");
			Requires.NotNull(clientAuthenticationModule, "clientAuthenticationModule");

			var bindingElements = new List<IChannelBindingElement>();

			// The order they are provided is used for outgoing messgaes, and reversed for incoming messages.
			bindingElements.Add(new MessageValidationBindingElement(clientAuthenticationModule));
			bindingElements.Add(new TokenCodeSerializationBindingElement());

			return bindingElements.ToArray();
		}
	}
}
