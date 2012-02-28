﻿//-----------------------------------------------------------------------
// <copyright file="OAuth2AuthorizationServerChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Net.Mime;
	using System.Web;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The channel for the OAuth protocol.
	/// </summary>
	internal class OAuth2AuthorizationServerChannel : OAuth2ChannelBase, IOAuth2ChannelWithAuthorizationServer {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2AuthorizationServerChannel"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		protected internal OAuth2AuthorizationServerChannel(IAuthorizationServer authorizationServer)
			: base(InitializeBindingElements(authorizationServer)) {
			Requires.NotNull(authorizationServer, "authorizationServer");
			this.AuthorizationServer = authorizationServer;
		}

		/// <summary>
		/// Gets the authorization server.
		/// </summary>
		/// <value>The authorization server.</value>
		public IAuthorizationServer AuthorizationServer { get; private set; }

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response.
		/// </summary>
		/// <param name="response">The response that is anticipated to contain an protocol message.</param>
		/// <returns>
		/// The deserialized message parts, if found.  Null otherwise.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown when the response is not valid.</exception>
		protected override IDictionary<string, string> ReadFromResponseCore(IncomingWebResponse response) {
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
		protected override OutgoingWebResponse PrepareDirectResponse(IProtocolMessage response) {
			var webResponse = new OutgoingWebResponse();
			this.ApplyMessageTemplate(response, webResponse);
			string json = this.SerializeAsJson(response);
			webResponse.SetResponse(json, new ContentType(JsonEncoded));
			return webResponse;
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="request">The request to search for an embedded message.</param>
		/// <returns>
		/// The deserialized message, if one is found.  Null otherwise.
		/// </returns>
		protected override IDirectedProtocolMessage ReadFromRequestCore(HttpRequestInfo request) {
			if (!string.IsNullOrEmpty(request.Url.Fragment)) {
				var fields = HttpUtility.ParseQueryString(request.Url.Fragment.Substring(1)).ToDictionary();

				MessageReceivingEndpoint recipient;
				try {
					recipient = request.GetRecipient();
				} catch (ArgumentException ex) {
					Logger.Messaging.WarnFormat("Unrecognized HTTP request: " + ex.ToString());
					return null;
				}

				return (IDirectedProtocolMessage)this.Receive(fields, recipient);
			}

			return base.ReadFromRequestCore(request);
		}

		/// <summary>
		/// Initializes the binding elements for the OAuth channel.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <returns>
		/// An array of binding elements used to initialize the channel.
		/// </returns>
		private static IChannelBindingElement[] InitializeBindingElements(IAuthorizationServer authorizationServer) {
			Requires.NotNull(authorizationServer, "authorizationServer");
			var bindingElements = new List<IChannelBindingElement>();

			bindingElements.Add(new AuthServerAllFlowsBindingElement());
			bindingElements.Add(new AuthorizationCodeBindingElement());
			bindingElements.Add(new AccessTokenBindingElement());
			bindingElements.Add(new AccessRequestBindingElement());

			return bindingElements.ToArray();
		}
	}
}
