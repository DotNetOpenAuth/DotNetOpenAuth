//-----------------------------------------------------------------------
// <copyright file="DirectResponseBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A common base class for OpenID direct message responses.
	/// </summary>
	[DebuggerDisplay("OpenID {Version} response")]
	internal class DirectResponseBase : IDirectResponseProtocolMessage {
		/// <summary>
		/// The openid.ns parameter in the message.
		/// </summary>
		/// <value>"http://specs.openid.net/auth/2.0" </value>
		/// <remarks>
		/// OpenID 2.0 Section 5.1.2: 
		/// This particular value MUST be present for the response to be a valid OpenID 2.0 response. 
		/// Future versions of the specification may define different values in order to allow message 
		/// recipients to properly interpret the request. 
		/// </remarks>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Read by reflection.")]
		[MessagePart("ns", IsRequired = true, AllowEmpty = false)]
#pragma warning disable 0414 // read by reflection
		private readonly string OpenIdNamespace = Protocol.OpenId2Namespace;
#pragma warning restore 0414

		/// <summary>
		/// Backing store for the <see cref="OriginatingRequest"/> properties.
		/// </summary>
		private IDirectedProtocolMessage originatingRequest;

		/// <summary>
		/// Backing store for the <see cref="Incoming"/> properties.
		/// </summary>
		private bool incoming;

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectResponseBase"/> class.
		/// </summary>
		/// <param name="originatingRequest">The originating request.</param>
		protected DirectResponseBase(IDirectedProtocolMessage originatingRequest) {
			ErrorUtilities.VerifyArgumentNotNull(originatingRequest, "originatingRequest");

			this.originatingRequest = originatingRequest;
			this.Version = originatingRequest.Version;
		}

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
		public MessageProtections RequiredProtection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		/// <value><see cref="MessageTransport.Direct"/></value>
		public MessageTransport Transport {
			get { return MessageTransport.Direct; }
		}

		/// <summary>
		/// Gets the extra, non-OAuth parameters included in the message.
		/// </summary>
		/// <value>An empty dictionary.</value>
		public IDictionary<string, string> ExtraData {
			get { return EmptyDictionary<string, string>.Instance; }
		}

		#endregion

		#region IDirectResponseProtocolMessage Members

		/// <summary>
		/// Gets the originating request message that caused this response to be formed.
		/// </summary>
		IDirectedProtocolMessage IDirectResponseProtocolMessage.OriginatingRequest {
			get { return this.originatingRequest; }
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

		/// <summary>
		/// Gets the originating request message that caused this response to be formed.
		/// </summary>
		protected IDirectedProtocolMessage OriginatingRequest {
			get { return this.originatingRequest; }
		}

		#region IProtocolMessage methods

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
	}
}
