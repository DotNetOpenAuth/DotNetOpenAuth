//-----------------------------------------------------------------------
// <copyright file="OAuthWrapChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// The channel for the OAuth WRAP protocol.
	/// </summary>
	internal class OAuthWrapResourceServerChannel : StandardMessageFactoryChannel {
		private static readonly Type[] MessageTypes = new Type[] {
			typeof(Messages.AccessProtectedResourceRequest),
		};

		private static readonly Version[] Versions = Protocol.AllVersions.Select(v => v.Version).ToArray();

		/// <summary>
		/// A character array containing just the = character.
		/// </summary>
		private static readonly char[] EqualsArray = new char[] { '=' };

		/// <summary>
		/// A character array containing just the , character.
		/// </summary>
		private static readonly char[] CommaArray = new char[] { ',' };

		/// <summary>
		/// A character array containing just the " character.
		/// </summary>
		private static readonly char[] QuoteArray = new char[] { '"' };

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthWrapResourceServerChannel"/> class.
		/// </summary>
		protected internal OAuthWrapResourceServerChannel()
			: base(MessageTypes, Versions) {
			// TODO: add signing (authenticated request) binding element.
		}

		private IEnumerable<KeyValuePair<string, string>> ParseAuthorizationHeader(string authorizationHeader) {
			const string Prefix = Protocol.HttpAuthorizationScheme + " ";
			if (authorizationHeader != null) {
				string[] authorizationSections = authorizationHeader.Split(';'); // TODO: is this the right delimiter?
				foreach (string authorization in authorizationSections) {
					if (authorizationHeader.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) {
						string data = authorizationHeader.Substring(Prefix.Length);
						return from element in data.Split(CommaArray)
							   let parts = element.Split(EqualsArray, 2)
							   let key = Uri.UnescapeDataString(parts[0])
							   let value = Uri.UnescapeDataString(parts[1].Trim(QuoteArray))
							   select new KeyValuePair<string, string>(key, value);
					}
				}
			}

			return Enumerable.Empty<KeyValuePair<string, string>>();
		}

		/// <summary>
		/// Gets the protocol message that may be embedded in the given HTTP request.
		/// </summary>
		/// <param name="request">The request to search for an embedded message.</param>
		/// <returns>
		/// The deserialized message, if one is found.  Null otherwise.
		/// </returns>
		protected override IDirectedProtocolMessage ReadFromRequestCore(HttpRequestInfo request) {
			var fields = new Dictionary<string, string>();

			// First search the Authorization header.
			var data = this.ParseAuthorizationHeader(request.Headers[HttpRequestHeader.Authorization])
				.ToDictionary(pair => pair.Key, pair => pair.Value);
			if (data.Count > 0) {
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

			return base.ReadFromRequestCore(request);
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
			throw new NotImplementedException();
		}

	}
}
