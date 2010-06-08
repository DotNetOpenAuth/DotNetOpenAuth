//-----------------------------------------------------------------------
// <copyright file="OAuthWrapResourceServerChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net;
	using System.Net.Mime;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuthWrap.Messages;

	/// <summary>
	/// The channel for the OAuth WRAP protocol.
	/// </summary>
	internal class OAuthWrapResourceServerChannel : StandardMessageFactoryChannel {
		/// <summary>
		/// The messages receivable by this channel.
		/// </summary>
		private static readonly Type[] MessageTypes = new Type[] {
			typeof(Messages.AccessProtectedResourceRequest),
		};

		/// <summary>
		/// The protocol versions supported by this channel.
		/// </summary>
		private static readonly Version[] Versions = Protocol.AllVersions.Select(v => v.Version).ToArray();

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthWrapResourceServerChannel"/> class.
		/// </summary>
		protected internal OAuthWrapResourceServerChannel()
			: base(MessageTypes, Versions) {
			// TODO: add signing (authenticated request) binding element.
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="request">The request to search for an embedded message.</param>
		/// <returns>
		/// The deserialized message, if one is found.  Null otherwise.
		/// </returns>
		protected override IDirectedProtocolMessage ReadFromRequestCore(HttpRequestInfo request) {
			// First search the Authorization header.
			var fields = MessagingUtilities.ParseAuthorizationHeader(
				Protocol.HttpAuthorizationScheme,
				request.Headers[HttpRequestHeader.Authorization]).ToDictionary();

			// Failing that, try the query string (for GET or POST or any other method)
			if (fields.Count == 0) {
				if (request.QueryStringBeforeRewriting["oauth_token"] != null) {
					// We're only interested in the oauth_token parameter -- not the others that can appear in an Authorization header.
					// Note that we intentionally change the name of the key here
					// because depending on the method used to obtain the token, the token's key changes
					// but we need to consolidate to one name so it works with the rest of the system.
					fields.Add("token", request.QueryStringBeforeRewriting["oauth_token"]);
				}
			}

			// Failing that, scan the entity
			if (fields.Count == 0) {
				// The spec calls out that this is allowed only for these three HTTP methods.
				if (request.HttpMethod == "POST" || request.HttpMethod == "DELETE" || request.HttpMethod == "PUT") {
					if (!string.IsNullOrEmpty(request.Headers[HttpRequestHeader.ContentType])) {
						var contentType = new ContentType(request.Headers[HttpRequestHeader.ContentType]);
						if (string.Equals(contentType.MediaType, HttpFormUrlEncoded, StringComparison.Ordinal)) {
							if (request.Form["oauth_token"] != null) {
								// We're only interested in the oauth_token parameter -- not the others that can appear in an Authorization header.
								// Note that we intentionally change the name of the key here
								// because depending on the method used to obtain the token, the token's key changes
								// but we need to consolidate to one name so it works with the rest of the system.
								fields.Add("token", request.Form["oauth_token"]);
							}
						}
					}
				}
			}

			if (fields.Count > 0) {
				MessageReceivingEndpoint recipient;
				try {
					recipient = request.GetRecipient();
				} catch (ArgumentException ex) {
					Logger.OAuth.WarnFormat("Unrecognized HTTP request: " + ex.ToString());
					return null;
				}

				// TODO: remove this after signatures are supported.
				ErrorUtilities.VerifyProtocol(!fields.ContainsKey("signature"), "OAuth signatures not supported yet.");

				// Deserialize the message using all the data we've collected.
				var message = (IDirectedProtocolMessage)this.Receive(fields, recipient);
				return message;
			}

			return null;
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
			// We never expect resource servers to send out direct requests,
			// and therefore won't have direct responses.
			throw new NotImplementedException();
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
			var webResponse = new OutgoingWebResponse();

			// The only direct response from a resource server is a 401 Unauthorized error.
			var unauthorizedResponse = response as UnauthorizedResponse;
			ErrorUtilities.VerifyInternal(unauthorizedResponse != null, "Only unauthorized responses are expected.");

			// First initialize based on the specifics within the message.
			var httpResponse = response as IHttpDirectResponse;
			webResponse.Status = httpResponse != null ? httpResponse.HttpStatusCode : HttpStatusCode.Unauthorized;
			foreach (string headerName in httpResponse.Headers) {
				webResponse.Headers.Add(headerName, httpResponse.Headers[headerName]);
			}

			// Now serialize all the message parts into the WWW-Authenticate header.
			var fields = this.MessageDescriptions.GetAccessor(response);
			webResponse.Headers[HttpResponseHeader.WwwAuthenticate] = MessagingUtilities.AssembleAuthorizationHeader(Protocol.HttpAuthorizationScheme, fields);
			return webResponse;
		}
	}
}
