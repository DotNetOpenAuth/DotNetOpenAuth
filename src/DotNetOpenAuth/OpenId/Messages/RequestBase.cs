//-----------------------------------------------------------------------
// <copyright file="RequestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A common base class for OpenID request messages.
	/// </summary>
	internal class RequestBase : IDirectedProtocolMessage {
		/// <summary>
		/// The openid.ns parameter in the message.
		/// </summary>
		/// <value>"http://specs.openid.net/auth/2.0" </value>
		/// <remarks>
		/// This particular value MUST be present for the request to be a valid OpenID Authentication 2.0 request. Future versions of the specification may define different values in order to allow message recipients to properly interpret the request. 
		/// </remarks>
		[MessagePart("openid.ns", IsRequired = true, AllowEmpty = false)]
#pragma warning disable 0414 // read by reflection
		private readonly string OpenIdNamespace = Protocol.OpenId2Namespace;
#pragma warning restore 0414

		/// <summary>
		/// Initializes a new instance of the <see cref="RequestBase"/> class.
		/// </summary>
		/// <param name="providerEndpoint">The OpenID Provider endpoint.</param>
		/// <param name="mode">The value for the openid.mode parameter.</param>
		/// <param name="transport">A value indicating whether the message will be transmitted directly or indirectly.</param>
		protected RequestBase(Uri providerEndpoint, string mode, MessageTransport transport) {
			if (providerEndpoint == null) {
				throw new ArgumentNullException("providerEndpoint");
			}
			if (String.IsNullOrEmpty(mode)) {
				throw new ArgumentNullException("mode");
			}

			this.Recipient = providerEndpoint;
			this.Mode = mode;
			this.Transport = transport;
		}

		/// <summary>
		/// Gets the value of the openid.mode parameter.
		/// </summary>
		[MessagePart("openid.mode", IsRequired = true, AllowEmpty = false)]
		public string Mode { get; private set; }

		#region IDirectedProtocolMessage Members

		/// <summary>
		/// Gets the recipient of the message.
		/// </summary>
		/// <value>The OP endpoint, or the RP return_to.</value>
		public Uri Recipient {
			get;
			private set;
		}

		#endregion

		#region IProtocolMessage Members

		/// <summary>
		/// Gets the version of the protocol this message is prepared to implement.
		/// </summary>
		/// <value>Version 2.0</value>
		public Version ProtocolVersion {
			get { return new Version(2, 0); }
		}

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		/// <value><see cref="MessageProtections.None"/></value>
		public MessageProtections RequiredProtection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		/// <value><see cref="MessageTransport.Direct"/></value>
		public MessageTransport Transport { get; private set; }

		/// <summary>
		/// Gets the extra, non-OAuth parameters included in the message.
		/// </summary>
		/// <value>An empty dictionary.</value>
		public IDictionary<string, string> ExtraData {
			get { return EmptyDictionary<string, string>.Instance; }
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
		public virtual void EnsureValidMessage() {
		}

		#endregion
	}
}
