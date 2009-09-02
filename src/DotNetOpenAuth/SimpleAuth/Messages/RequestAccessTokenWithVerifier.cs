//-----------------------------------------------------------------------
// <copyright file="RequestAccessTokenWithVerifier.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A message sent by the Consumer directly to the Token Issuer to exchange
	/// the verifier code for an Access Token.
	/// </summary>
	internal class RequestAccessTokenWithVerifier : IDirectedProtocolMessage {
		/// <summary>
		/// A dictionary to contain extra message data.
		/// </summary>
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="RequestAccessTokenWithVerifier"/> class.
		/// </summary>
		/// <param name="tokenIssuer">The token issuer.</param>
		/// <param name="version">The version.</param>
		internal RequestAccessTokenWithVerifier(Uri tokenIssuer, Version version) {
			Contract.Requires<ArgumentNullException>(tokenIssuer != null);
			Contract.Requires<ArgumentNullException>(version != null);

			this.Recipient = tokenIssuer;
			this.Version = version;
		}

		#region IDirectedProtocolMessage Members

		/// <summary>
		/// Gets the preferred method of transport for the message.
		/// </summary>
		/// <value><see cref="HttpDeliveryMethods.PostRequest"/></value>
		/// <remarks>
		/// For indirect messages this will likely be GET+POST, which both can be simulated in the user agent:
		/// the GET with a simple 301 Redirect, and the POST with an HTML form in the response with javascript
		/// to automate submission.
		/// </remarks>
		public HttpDeliveryMethods HttpMethods {
			get { return HttpDeliveryMethods.PostRequest; }
		}

		/// <summary>
		/// Gets the URL of the intended receiver of this message.
		/// </summary>
		public Uri Recipient { get; private set; }

		#endregion

		#region IProtocolMessage Members

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		/// <value><see cref="MessageProtections.TamperProtection"/></value>
		/// <remarks>
		/// The protection this message requires is provided by the mandated use of HTTPS.
		/// </remarks>
		public MessageProtections RequiredProtection {
			get { return MessageProtections.TamperProtection; }
		}

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		/// <value><see cref="MessageTransport.Direct"/></value>
		public MessageTransport Transport {
			get { return MessageTransport.Direct; }
		}

		#endregion

		#region IMessage Members

		/// <summary>
		/// Gets the version of the protocol or extension this message is prepared to implement.
		/// </summary>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		public Version Version { get; private set; }

		/// <summary>
		/// Gets the extra, non-standard Protocol parameters included in the message.
		/// </summary>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		public IDictionary<string, string> ExtraData {
			get { return this.extraData; }
		}

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// 	<para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the
		/// message to see if it conforms to the protocol.</para>
		/// 	<para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		public void EnsureValidMessage() {
			ErrorUtilities.VerifyProtocol(this.Recipient.IsTransportSecure(), SimpleAuthStrings.HttpsRequired);
		}

		#endregion
	}
}
