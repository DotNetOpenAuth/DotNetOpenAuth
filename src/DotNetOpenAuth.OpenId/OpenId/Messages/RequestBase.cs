//-----------------------------------------------------------------------
// <copyright file="RequestBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A common base class for OpenID request messages and indirect responses (since they are ultimately requests).
	/// </summary>
	[DebuggerDisplay("OpenID {Version} {Mode}")]
	[Serializable]
	internal class RequestBase : IDirectedProtocolMessage {
		/// <summary>
		/// The openid.ns parameter in the message.
		/// </summary>
		/// <value>"http://specs.openid.net/auth/2.0" </value>
		/// <remarks>
		/// This particular value MUST be present for the request to be a valid OpenID Authentication 2.0 request. Future versions of the specification may define different values in order to allow message recipients to properly interpret the request. 
		/// </remarks>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Read by reflection.")]
		[MessagePart("openid.ns", IsRequired = true, AllowEmpty = false, MinVersion = "2.0")]
#pragma warning disable 0414 // read by reflection
		private readonly string OpenIdNamespace = Protocol.OpenId2Namespace;
#pragma warning restore 0414

		/// <summary>
		/// Backing store for the <see cref="ExtraData"/> property.
		/// </summary>
		private readonly Dictionary<string, string> extraData = new Dictionary<string, string>();

		/// <summary>
		/// Backing store for the <see cref="Incoming"/> property.
		/// </summary>
		private bool incoming;

		/// <summary>
		/// Initializes a new instance of the <see cref="RequestBase"/> class.
		/// </summary>
		/// <param name="version">The OpenID version this message must comply with.</param>
		/// <param name="providerEndpoint">The OpenID Provider endpoint.</param>
		/// <param name="mode">The value for the openid.mode parameter.</param>
		/// <param name="transport">A value indicating whether the message will be transmitted directly or indirectly.</param>
		protected RequestBase(Version version, Uri providerEndpoint, string mode, MessageTransport transport) {
			Requires.NotNull(providerEndpoint, "providerEndpoint");
			Requires.NotNullOrEmpty(mode, "mode");

			this.Recipient = providerEndpoint;
			this.Mode = mode;
			this.Transport = transport;
			this.Version = version;
		}

		/// <summary>
		/// Gets the value of the openid.mode parameter.
		/// </summary>
		[MessagePart("openid.mode", IsRequired = true, AllowEmpty = false)]
		public string Mode { get; private set; }

		#region IDirectedProtocolMessage Members

		/// <summary>
		/// Gets the preferred method of transport for the message.
		/// </summary>
		/// <value>
		/// For direct messages this is the OpenID mandated POST.  
		/// For indirect messages both GET and POST are allowed.
		/// </value>
		HttpDeliveryMethods IDirectedProtocolMessage.HttpMethods {
			get {
				// OpenID 2.0 section 5.1.1
				HttpDeliveryMethods methods = HttpDeliveryMethods.PostRequest;
				if (this.Transport == MessageTransport.Indirect) {
					methods |= HttpDeliveryMethods.GetRequest;
				}
				return methods;
			}
		}

		/// <summary>
		/// Gets the recipient of the message.
		/// </summary>
		/// <value>The OP endpoint, or the RP return_to.</value>
		public Uri Recipient {
			get;
			private set;
		}

		#endregion

		#region IProtocolMessage Properties

		/// <summary>
		/// Gets the version of the protocol this message is prepared to implement.
		/// </summary>
		/// <value>Version 2.0</value>
		public Version Version { get; private set; }

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		/// <value><see cref="MessageProtections.None"/></value>
		public virtual MessageProtections RequiredProtection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		/// <value><see cref="MessageTransport.Direct"/></value>
		public MessageTransport Transport { get; private set; }

		/// <summary>
		/// Gets the extra parameters included in the message.
		/// </summary>
		/// <value>An empty dictionary.</value>
		public IDictionary<string, string> ExtraData {
			get { return this.extraData; }
		}

		#endregion

		/// <summary>
		/// Gets a value indicating whether this message was deserialized as an incoming message.
		/// </summary>
		protected internal bool Incoming {
			get { return this.incoming; }
		}

		/// <summary>
		/// Gets the protocol used by this message.
		/// </summary>
		protected Protocol Protocol {
			get { return Protocol.Lookup(this.Version); }
		}

		#region IProtocolMessage Methods

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

		/// <summary>
		/// Sets a flag indicating that this message is received (as opposed to sent).
		/// </summary>
		internal void SetAsIncoming() {
			this.incoming = true;
		}

		/// <summary>
		/// Gets some string from a given version of the OpenID protocol.
		/// </summary>
		/// <param name="protocolVersion">The protocol version to use for lookup.</param>
		/// <param name="mode">A function that can retrieve the desired protocol constant.</param>
		/// <returns>The value of the constant.</returns>
		/// <remarks>
		/// This method can be used by a constructor to throw an <see cref="ArgumentNullException"/>
		/// instead of a <see cref="NullReferenceException"/>.
		/// </remarks>
		protected static string GetProtocolConstant(Version protocolVersion, Func<Protocol, string> mode) {
			Requires.NotNull(protocolVersion, "protocolVersion");
			return mode(Protocol.Lookup(protocolVersion));
		}
	}
}
