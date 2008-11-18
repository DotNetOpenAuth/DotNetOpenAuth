//-----------------------------------------------------------------------
// <copyright file="AssociateRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An OpenID direct request from Relying Party to Provider to initiate an association.
	/// </summary>
	[DebuggerDisplay("OpenID {ProtocolVersion} {Mode} {AssociationType} {SessionType}")]
	internal abstract class AssociateRequest : RequestBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateRequest"/> class.
		/// </summary>
		/// <param name="providerEndpoint">The OpenID Provider endpoint.</param>
		protected AssociateRequest(Uri providerEndpoint)
			: base(providerEndpoint, "associate", MessageTransport.Direct) {
		}

		/// <summary>
		/// Gets or sets the preferred association type. The association type defines the algorithm to be used to sign subsequent messages. 
		/// </summary>
		/// <value>Value: A valid association type from Section 8.3.</value>
		[MessagePart("openid.assoc_type", IsRequired = true, AllowEmpty = false)]
		internal string AssociationType { get; set; }

		/// <summary>
		/// Gets or sets the preferred association session type. This defines the method used to encrypt the association's MAC key in transit. 
		/// </summary>
		/// <value>Value: A valid association session type from Section 8.4 (Association Session Types). </value>
		/// <remarks>Note: Unless using transport layer encryption, "no-encryption" MUST NOT be used. </remarks>
		[MessagePart("openid.session_type", IsRequired = true, AllowEmpty = false)]
		internal string SessionType { get; set; }

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
		public override void EnsureValidMessage() {
			base.EnsureValidMessage();

			ErrorUtilities.Verify(
				!string.Equals(this.SessionType, Protocol.Args.SessionType.NoEncryption, StringComparison.Ordinal) || this.Recipient.IsTransportSecure(),
				OpenIdStrings.NoEncryptionSessionRequiresHttps,
				this);
		}

		/// <summary>
		/// Creates an association request message that is appropriate for a given Provider.
		/// </summary>
		/// <param name="provider">The provider to create an association with.</param>
		/// <returns>The message to send to the Provider to request an association.</returns>
		internal static AssociateRequest Create(ProviderEndpointDescription provider) {
			AssociateRequest associateRequest;
			if (provider.Endpoint.IsTransportSecure()) {
				associateRequest = new AssociateUnencryptedRequest(provider.Endpoint);
			} else {
				// TODO: apply security policies and our knowledge of the provider's OpenID version
				// to select the right association here.
				var diffieHellmanAssociateRequest = new AssociateDiffieHellmanRequest(provider.Endpoint);
				diffieHellmanAssociateRequest.AssociationType = provider.Protocol.Args.SignatureAlgorithm.HMAC_SHA1;
				diffieHellmanAssociateRequest.SessionType = provider.Protocol.Args.SessionType.DH_SHA1;
				diffieHellmanAssociateRequest.InitializeRequest();
				associateRequest = diffieHellmanAssociateRequest;
			}

			return associateRequest;
		}
	}
}
