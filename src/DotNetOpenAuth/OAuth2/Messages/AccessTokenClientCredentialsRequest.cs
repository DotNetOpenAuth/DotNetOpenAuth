//-----------------------------------------------------------------------
// <copyright file="AccessTokenClientCredentialsRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// A request for an access token for a client application that has its
	/// own (non-user affiliated) client name and password.
	/// </summary>
	/// <remarks>
	/// This is somewhat analogous to 2-legged OAuth.
	/// </remarks>
	internal class AccessTokenClientCredentialsRequest : AccessTokenRequestBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenClientCredentialsRequest"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal AccessTokenClientCredentialsRequest(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
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
			ErrorUtilities.VerifyProtocol(this.Recipient.IsTransportSecure(), OAuthWrapStrings.HttpsRequired);
		}

		internal override GrantType GrantType {
			get { return Messages.GrantType.None; }
		}
	}
}
