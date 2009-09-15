//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationViaUsernamePasswordRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A request for a delegation code in exchnage for a user's confidential 
	/// username and password.
	/// </summary>
	/// <remarks>
	/// After this request has been sent, the consumer application MUST discard
	/// the confidential user credentials and use the delegation code going forward.
	/// </remarks>
	internal class UserAuthorizationViaUsernamePasswordRequest : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationViaUsernamePasswordRequest"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		internal UserAuthorizationViaUsernamePasswordRequest(Version version)
			: base(version) {
		}

		/// <summary>
		/// Gets or sets the consumer key.
		/// </summary>
		/// <value>The consumer key.</value>
		[MessagePart(Protocol.sa_consumer_key, IsRequired = true, AllowEmpty = false)]
		internal string ConsumerKey { get; set; }

		/// <summary>
		/// Gets or sets the consumer secret.
		/// </summary>
		/// <value>The consumer secret.</value>
		[MessagePart(Protocol.sa_consumer_secret, IsRequired = true, AllowEmpty = false)]
		internal string ConsumerSecret { get; set; }

		/// <summary>
		/// Gets or sets the username.
		/// </summary>
		/// <value>The name of the user.</value>
		[MessagePart(Protocol.sa_username, IsRequired = true, AllowEmpty = false)]
		internal string UserName { get; set; }

		/// <summary>
		/// Gets or sets the user's password.
		/// </summary>
		/// <value>The password.</value>
		[MessagePart(Protocol.sa_password, IsRequired = true, AllowEmpty = false)]
		internal string Password { get; set; }

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
			ErrorUtilities.VerifyProtocol(this.Recipient.IsTransportSecure(), SimpleAuthStrings.HttpsRequired);
		}
	}
}
