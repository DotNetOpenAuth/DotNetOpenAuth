//-----------------------------------------------------------------------
// <copyright file="AccessProtectedResourceRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ChannelElements;
	using Messaging;

	internal class AccessProtectedResourceRequest : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessProtectedResourceRequest"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="recipient">The recipient.</param>
		internal AccessProtectedResourceRequest(Version version, Uri recipient)
			: base(version, MessageTransport.Direct, recipient) {
		}

		[MessagePart("token", IsRequired = true, AllowEmpty = false)]
		internal string AccessToken { get; set; }

		[MessagePart("nonce")]
		internal string Nonce { get; set; }

		[MessagePart("timestamp", Encoder = typeof(TimestampEncoder))]
		internal DateTime? Timestamp { get; set; }

		[MessagePart("signature")]
		internal string Signature { get; set; }

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
