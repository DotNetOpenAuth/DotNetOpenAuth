//-----------------------------------------------------------------------
// <copyright file="OAuth2ClientChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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

	internal class OAuth2ClientChannel : OAuth2ChannelBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2ClientChannel"/> class.
		/// </summary>
		internal OAuth2ClientChannel() {
		}

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

		protected override IDirectedProtocolMessage ReadFromRequestCore(HttpRequestInfo request) {
			Logger.Channel.DebugFormat("Incoming HTTP request: {0} {1}", request.HttpMethod, request.UrlBeforeRewriting.AbsoluteUri);

			var fields = request.QueryStringBeforeRewriting.ToDictionary();

			// Also read parameters from the fragment, if it's available.
			// Typically the fragment is not available because the browser doesn't send it to a web server
			// but this request may have been fabricated by an installed desktop app, in which case
			// the fragment is available.
			string fragment = request.UrlBeforeRewriting.Fragment;
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

		protected override OutgoingWebResponse PrepareDirectResponse(IProtocolMessage response) {
			throw new NotImplementedException();
		}
	}
}
