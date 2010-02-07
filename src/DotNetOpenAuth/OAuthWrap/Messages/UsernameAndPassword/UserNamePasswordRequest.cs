//-----------------------------------------------------------------------
// <copyright file="UserNamePasswordRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
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
	internal class UserNamePasswordRequest : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserNamePasswordRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal UserNamePasswordRequest(Uri authorizationServer, Version version)
			: base(version, MessageTransport.Direct, authorizationServer) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		/// <summary>
		/// Gets or sets the client identifier previously obtained from the Authorization Server.
		/// </summary>
		/// <value>The client identifier.</value>
		[MessagePart(Protocol.wrap_client_id, IsRequired = true, AllowEmpty = false)]
		internal string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the user's account username.
		/// </summary>
		/// <value>The username on the user's account.</value>
		[MessagePart(Protocol.wrap_username, IsRequired = true, AllowEmpty = false)]
		internal string UserName { get; set; }

		/// <summary>
		/// Gets or sets the user's password.
		/// </summary>
		/// <value>The password.</value>
		[MessagePart(Protocol.wrap_password, IsRequired = true, AllowEmpty = false)]
		internal string Password { get; set; }

		/// <summary>
		/// Gets or sets the CAPTCHA puzzle that the user just solved, if applicable.
		/// </summary>
		/// <value>The captcha puzzle location.</value>
		[MessagePart(Protocol.wrap_captcha_url, IsRequired = false, AllowEmpty = false)]
		internal Uri CaptchaPuzzle { get; set; }

		/// <summary>
		/// Gets or sets the solution to the CAPTCHA puzzle the user just solved, if applicable.
		/// </summary>
		/// <value>The CAPTCHA solution.</value>
		[MessagePart(Protocol.wrap_captcha_solution, IsRequired = false, AllowEmpty = false)]
		internal string CaptchaSolution { get; set; }

		/// <summary>
		/// Gets or sets the scope.
		/// </summary>
		/// <value>The scope.</value>
		[MessagePart(Protocol.wrap_scope, IsRequired = false, AllowEmpty = true)]
		internal string Scope { get; set; }

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
