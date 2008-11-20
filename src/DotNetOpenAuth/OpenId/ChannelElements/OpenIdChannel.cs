//-----------------------------------------------------------------------
// <copyright file="OpenIdChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A channel that knows how to send and receive OpenID messages.
	/// </summary>
	internal class OpenIdChannel : Channel {
		/// <summary>
		/// The HTTP Content-Type to use in Key-Value Form responses.
		/// </summary>
		/// <remarks>
		/// OpenID 2.0 section 5.1.2 says this SHOULD be text/plain.  But this value 
		/// does not prevent free hosters like GoDaddy from tacking on their ads
		/// to the end of the direct response, corrupting the data.  So we deviate
		/// from the spec a bit here to improve the story for free Providers.
		/// </remarks>
		private const string KeyValueFormContentType = "application/x-openid-kvf";

		/// <summary>
		/// The encoder that understands how to read and write Key-Value Form.
		/// </summary>
		private KeyValueFormEncoding keyValueForm = new KeyValueFormEncoding();

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdChannel"/> class.
		/// </summary>
		internal OpenIdChannel()
			: this(new OpenIdMessageFactory()) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdChannel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">An object that knows how to distinguish the various OpenID message types for deserialization purposes.</param>
		private OpenIdChannel(IMessageFactory messageTypeProvider) :
			base(messageTypeProvider) {
		}

		/// <summary>
		/// Prepares an HTTP request that carries a given message.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>
		/// The <see cref="HttpWebRequest"/> prepared to send the request.
		/// </returns>
		protected override HttpWebRequest CreateHttpRequest(IDirectedProtocolMessage request) {
			return this.InitializeRequestAsPost(request);
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response.
		/// </summary>
		/// <param name="response">The response that is anticipated to contain an protocol message.</param>
		/// <returns>
		/// The deserialized message parts, if found.  Null otherwise.
		/// </returns>
		protected override IDictionary<string, string> ReadFromResponseInternal(Response response) {
			if (response == null) {
				throw new ArgumentNullException("response");
			}

			return this.keyValueForm.GetDictionary(response.ResponseStream);
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
		/// This method implements spec V1.0 section 5.3.
		/// </remarks>
		protected override Response SendDirectMessageResponse(IProtocolMessage response) {
			if (response == null) {
				throw new ArgumentNullException("response");
			}

			var serializer = MessageSerializer.Get(response.GetType());
			var fields = serializer.Serialize(response);
			byte[] keyValueEncoding = KeyValueFormEncoding.GetBytes(fields);

			Response preparedResponse = new Response();
			preparedResponse.Headers.Add(HttpResponseHeader.ContentType, KeyValueFormContentType);
			preparedResponse.OriginalMessage = response;
			preparedResponse.ResponseStream = new MemoryStream(keyValueEncoding);

			return preparedResponse;
		}
	}
}
