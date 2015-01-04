//-----------------------------------------------------------------------
// <copyright file="OAuth2ResourceServerChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Net.Mime;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth2.Messages;
	using Validation;
	using HttpRequestHeaders = DotNetOpenAuth.Messaging.HttpRequestHeaders;

	/// <summary>
	/// The channel for the OAuth protocol.
	/// </summary>
	internal class OAuth2ResourceServerChannel : StandardMessageFactoryChannel {
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
		/// Initializes a new instance of the <see cref="OAuth2ResourceServerChannel" /> class.
		/// </summary>
		/// <param name="hostFactories">The host factories.</param>
		protected internal OAuth2ResourceServerChannel(IHostFactories hostFactories = null)
			: base(MessageTypes, Versions, hostFactories ?? new OAuth.DefaultOAuthHostFactories()) {
			// TODO: add signing (authenticated request) binding element.
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

			var fields = new Dictionary<string, string>();
			string accessToken;
			if ((accessToken = await SearchForBearerAccessTokenInRequestAsync(request, cancellationToken)) != null) {
				fields[Protocol.token_type] = Protocol.AccessTokenTypes.Bearer;
				fields[Protocol.access_token] = accessToken;
			}

			if (fields.Count > 0) {
				MessageReceivingEndpoint recipient;
				try {
					recipient = request.GetRecipient();
				} catch (ArgumentException ex) {
					Logger.OAuth.WarnFormat("Unrecognized HTTP request: " + ex.ToString());
					return null;
				}

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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The deserialized message parts, if found.  Null otherwise.
		/// </returns>
		/// <exception cref="System.NotImplementedException">Always thrown.</exception>
		/// <exception cref="ProtocolException">Thrown when the response is not valid.</exception>
		protected override Task<IDictionary<string, string>> ReadFromResponseCoreAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
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
		protected override HttpResponseMessage PrepareDirectResponse(IProtocolMessage response) {
			var webResponse = new HttpResponseMessage();

			// The only direct response from a resource server is some authorization error (400, 401, 403).
			var unauthorizedResponse = response as UnauthorizedResponse;
			ErrorUtilities.VerifyInternal(unauthorizedResponse != null, "Only unauthorized responses are expected.");

			// First initialize based on the specifics within the message.
			ApplyMessageTemplate(response, webResponse);
			if (!(response is IHttpDirectResponse)) {
				webResponse.StatusCode = HttpStatusCode.Unauthorized;
			}

			// Now serialize all the message parts into the WWW-Authenticate header.
			var fields = this.MessageDescriptions.GetAccessor(response);
			webResponse.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(unauthorizedResponse.Scheme, MessagingUtilities.AssembleAuthorizationHeader(fields)));
			return webResponse;
		}

		/// <summary>
		/// Searches for a bearer access token in the request.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The bearer access token, if one exists.  Otherwise <c>null</c>.
		/// </returns>
		private static async Task<string> SearchForBearerAccessTokenInRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			Requires.NotNull(request, "request");

			// First search the authorization header.
			var authorizationHeader = request.Headers.Authorization;
			if (authorizationHeader != null && string.Equals(authorizationHeader.Scheme, Protocol.BearerHttpAuthorizationScheme, StringComparison.OrdinalIgnoreCase)) {
				return authorizationHeader.Parameter;
			}

			// Failing that, scan the entity
			foreach (var pair in await ParseUrlEncodedFormContentAsync(request, cancellationToken)) {
				if (string.Equals(pair.Key, Protocol.BearerTokenEncodedUrlParameterName, StringComparison.Ordinal)) {
					return pair.Value;
				}
			}

			// Finally, check the least desirable location: the query string
			var unrewrittenQuery = HttpUtility.ParseQueryString(request.RequestUri.Query);
			if (!string.IsNullOrEmpty(unrewrittenQuery[Protocol.BearerTokenEncodedUrlParameterName])) {
				return unrewrittenQuery[Protocol.BearerTokenEncodedUrlParameterName];
			}

			return null;
		}
	}
}
