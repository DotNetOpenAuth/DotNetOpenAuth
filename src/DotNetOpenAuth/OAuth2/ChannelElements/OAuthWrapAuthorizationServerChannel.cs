//-----------------------------------------------------------------------
// <copyright file="OAuthWrapAuthorizationServerChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net;
	using System.Net.Mime;
	using System.Runtime.Serialization.Json;
	using System.Security.Cryptography;
	using System.Text;
	using System.Web;
	using System.Web.Script.Serialization;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// The channel for the OAuth protocol.
	/// </summary>
	internal class OAuthWrapAuthorizationServerChannel : StandardMessageFactoryChannel {
		/// <summary>
		/// The messages receivable by this channel.
		/// </summary>
		private static readonly Type[] MessageTypes = new Type[] {
			typeof(AccessTokenRefreshRequest),
			typeof(AccessTokenAuthorizationCodeRequest),
			typeof(AccessTokenBasicCredentialsRequest),
			typeof(AccessTokenAssertionRequest),
			typeof(AccessTokenClientCredentialsRequest),
			typeof(AccessTokenSuccessResponse),
			typeof(AccessTokenFailedResponse),
			typeof(EndUserAuthorizationRequest),
			typeof(EndUserAuthorizationSuccessResponse),
			typeof(EndUserAuthorizationFailedResponse),
			typeof(UnauthorizedResponse),
		};

		/// <summary>
		/// The protocol versions supported by this channel.
		/// </summary>
		private static readonly Version[] Versions = Protocol.AllVersions.Select(v => v.Version).ToArray();

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthWrapAuthorizationServerChannel"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		protected internal OAuthWrapAuthorizationServerChannel(IAuthorizationServer authorizationServer = null)
			: base(MessageTypes, Versions, InitializeBindingElements(authorizationServer)) {
			this.AuthorizationServer = authorizationServer;
		}

		/// <summary>
		/// Gets the authorization server.
		/// </summary>
		/// <value>The authorization server.  Will be null for channels serving clients.</value>
		public IAuthorizationServer AuthorizationServer { get; private set; }

		/// <summary>
		/// Prepares an HTTP request that carries a given message.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>
		/// The <see cref="HttpWebRequest"/> prepared to send the request.
		/// </returns>
		/// <remarks>
		/// This method must be overridden by a derived class, unless the <see cref="Channel.RequestCore"/> method
		/// is overridden and does not require this method.
		/// </remarks>
		protected override HttpWebRequest CreateHttpRequest(IDirectedProtocolMessage request) {
			HttpWebRequest httpRequest;
			if ((request.HttpMethods & HttpDeliveryMethods.GetRequest) != 0) {
				httpRequest = InitializeRequestAsGet(request);
			} else if ((request.HttpMethods & HttpDeliveryMethods.PostRequest) != 0) {
				httpRequest = InitializeRequestAsPost(request);
			} else {
				throw new NotSupportedException();
			}

			return httpRequest;
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response.
		/// </summary>
		/// <param name="response">The response that is anticipated to contain an protocol message.</param>
		/// <returns>
		/// The deserialized message parts, if found.  Null otherwise.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown when the response is not valid.</exception>
		protected override IDictionary<string, string> ReadFromResponseCore(IncomingWebResponse response) {
			// The spec says direct responses should be JSON objects, but Facebook uses HttpFormUrlEncoded instead, calling it text/plain
			string body = response.GetResponseReader().ReadToEnd();
			if (response.ContentType.MediaType == JsonEncoded) {
				return this.DeserializeFromJson(body);
			} else if (response.ContentType.MediaType == HttpFormUrlEncoded || response.ContentType.MediaType == PlainTextEncoded) {
				return HttpUtility.ParseQueryString(body).ToDictionary();
			} else {
				throw ErrorUtilities.ThrowProtocol("Unexpected response Content-Type {0}", response.ContentType.MediaType);
			}
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
			var bindingElements = new List<IChannelBindingElement>();

			if (authorizationServer != null) {
				bindingElements.Add(new AuthServerAllFlowsBindingElement());
				bindingElements.Add(new WebServerVerificationCodeBindingElement());
				bindingElements.Add(new AccessRequestBindingElement());
			}

			return bindingElements.ToArray();
		}
	}
}
