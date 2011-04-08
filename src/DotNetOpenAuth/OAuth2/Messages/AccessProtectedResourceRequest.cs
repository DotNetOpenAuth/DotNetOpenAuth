//-----------------------------------------------------------------------
// <copyright file="AccessProtectedResourceRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ChannelElements;
	using Messaging;

	/// <summary>
	/// A message that accompanies an HTTP request to a resource server that provides authorization.
	/// </summary>
	internal class AccessProtectedResourceRequest : MessageBase, ITokenCarryingRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessProtectedResourceRequest"/> class.
		/// </summary>
		/// <param name="recipient">The recipient.</param>
		/// <param name="version">The version.</param>
		internal AccessProtectedResourceRequest(Uri recipient, Version version)
			: base(version, MessageTransport.Direct, recipient) {
		}

		/// <summary>
		/// Gets the type of the code or token.
		/// </summary>
		/// <value>The type of the code or token.</value>
		CodeOrTokenType ITokenCarryingRequest.CodeOrTokenType {
			get { return CodeOrTokenType.AccessToken; }
		}

		/// <summary>
		/// Gets or sets the verification code or refresh/access token.
		/// </summary>
		/// <value>The code or token.</value>
		string ITokenCarryingRequest.CodeOrToken {
			get { return this.AccessToken; }
			set { this.AccessToken = value; }
		}

		/// <summary>
		/// Gets or sets the authorization that the token describes.
		/// </summary>
		IAuthorizationDescription ITokenCarryingRequest.AuthorizationDescription { get; set; }

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>The access token.</value>
		[MessagePart("token", IsRequired = true)]
		internal string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the nonce.
		/// </summary>
		/// <value>The nonce.</value>
		[MessagePart("nonce")]
		internal string Nonce { get; set; }

		/// <summary>
		/// Gets or sets the timestamp.
		/// </summary>
		/// <value>The timestamp.</value>
		[MessagePart("timestamp", Encoder = typeof(TimestampEncoder))]
		internal DateTime? Timestamp { get; set; }

		/// <summary>
		/// Gets or sets the signature.
		/// </summary>
		/// <value>The signature.</value>
		[MessagePart("signature")]
		internal string Signature { get; set; }

		/// <summary>
		/// Gets or sets the algorithm.
		/// </summary>
		/// <value>The algorithm.</value>
		[MessagePart("algorithm")]
		internal string Algorithm { get; set; }

		/// <summary>
		/// Gets a value indicating whether this request is signed.
		/// </summary>
		internal bool SignedRequest {
			get { return this.Signature != null; }
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
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();

			// If any of the optional parameters are present, all of them are required.
			if (this.Signature == null) {
				ErrorUtilities.VerifyProtocol(this.Algorithm == null, this, MessagingStrings.UnexpectedMessagePartValue, "algorithm", this.Algorithm);
				ErrorUtilities.VerifyProtocol(!this.Timestamp.HasValue, this, MessagingStrings.UnexpectedMessagePartValue, "timestamp", this.Timestamp);
				ErrorUtilities.VerifyProtocol(this.Nonce == null, this, MessagingStrings.UnexpectedMessagePartValue, "nonce", this.Nonce);
			} else {
				ErrorUtilities.VerifyProtocol(this.Algorithm != null, this, MessagingStrings.RequiredParametersMissing, "algorithm");
				ErrorUtilities.VerifyProtocol(this.Timestamp.HasValue, this, MessagingStrings.RequiredParametersMissing, "timestamp");
				ErrorUtilities.VerifyProtocol(this.Nonce != null, this, MessagingStrings.RequiredParametersMissing, "nonce");
			}
		}
	}
}
