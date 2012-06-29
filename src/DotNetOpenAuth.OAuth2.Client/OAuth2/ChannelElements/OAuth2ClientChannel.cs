//-----------------------------------------------------------------------
// <copyright file="OAuth2ClientChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.Contracts;
	using System.Net;
	using System.Web;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// The messaging channel used by OAuth 2.0 Clients.
	/// </summary>
	internal class OAuth2ClientChannel : OAuth2ChannelBase, IOAuth2ChannelWithClient {
		/// <summary>
		/// The messages receivable by this channel.
		/// </summary>
		private static readonly Type[] MessageTypes = new Type[] {
			typeof(AccessTokenSuccessResponse),
			typeof(AccessTokenFailedResponse),
			typeof(EndUserAuthorizationSuccessAuthCodeResponse),
			typeof(EndUserAuthorizationSuccessAccessTokenResponse),
			typeof(EndUserAuthorizationFailedResponse),
			typeof(UnauthorizedResponse),
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2ClientChannel"/> class.
		/// </summary>
		internal OAuth2ClientChannel()
			: base(MessageTypes) {
		}

		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		public string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the tool to use to apply client credentials to authenticated requests to the Authorization Server.
		/// </summary>
		/// <value>May be <c>null</c> if this client has no client secret.</value>
		public ClientCredentialApplicator ClientCredentialApplicator { get; set; }

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
			// Others return text/javascript.  Again bad.
			string body = response.GetResponseReader().ReadToEnd();
			if (response.ContentType.MediaType == JsonEncoded || response.ContentType.MediaType == JsonTextEncoded) {
				return this.DeserializeFromJson(body);
			} else if (response.ContentType.MediaType == HttpFormUrlEncoded || response.ContentType.MediaType == PlainTextEncoded) {
				return HttpUtility.ParseQueryString(body).ToDictionary();
			} else {
				throw ErrorUtilities.ThrowProtocol(ClientStrings.UnexpectedResponseContentType, response.ContentType.MediaType);
			}
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="request">The request to search for an embedded message.</param>
		/// <returns>
		/// The deserialized message, if one is found.  Null otherwise.
		/// </returns>
		protected override IDirectedProtocolMessage ReadFromRequestCore(HttpRequestBase request) {
			Logger.Channel.DebugFormat("Incoming HTTP request: {0} {1}", request.HttpMethod, request.GetPublicFacingUrl().AbsoluteUri);

			var fields = request.GetQueryStringBeforeRewriting().ToDictionary();

			// Also read parameters from the fragment, if it's available.
			// Typically the fragment is not available because the browser doesn't send it to a web server
			// but this request may have been fabricated by an installed desktop app, in which case
			// the fragment is available.
			string fragment = request.GetPublicFacingUrl().Fragment;
			if (!string.IsNullOrEmpty(fragment)) {
				foreach (var pair in HttpUtility.ParseQueryString(fragment.Substring(1)).ToDictionary()) {
					fields.Add(pair.Key, pair.Value);
				}
			}

			MessageReceivingEndpoint recipient;
			try {
				recipient = request.GetRecipient();
			} catch (ArgumentException ex) {
				Logger.Messaging.WarnFormat("Unrecognized HTTP request: ", ex);
				return null;
			}

			return (IDirectedProtocolMessage)this.Receive(fields, recipient);
		}

		/// <summary>
		/// Queues a message for sending in the response stream where the fields
		/// are sent in the response stream in querystring style.
		/// </summary>
		/// <param name="response">The message to send as a response.</param>
		/// <returns>
		/// The pending user agent redirect based message to be sent as an HttpResponse.
		/// </returns>
		/// <remarks>
		/// This method implements spec OAuth V1.0 section 5.3.
		/// </remarks>
		protected override OutgoingWebResponse PrepareDirectResponse(IProtocolMessage response) {
			// Clients don't ever send direct responses.
			throw new NotImplementedException();
		}

		/// <summary>
		/// Performs additional processing on an outgoing web request before it is sent to the remote server.
		/// </summary>
		/// <param name="request">The request.</param>
		protected override void PrepareHttpWebRequest(HttpWebRequest request) {
			base.PrepareHttpWebRequest(request);

			if (this.ClientCredentialApplicator != null) {
				this.ClientCredentialApplicator.ApplyClientCredential(this.ClientIdentifier, request);
			}
		}
	}
}
