//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A message used to redirect the user from a Consumer to a Service Provider's web site
	/// so the Service Provider can ask the user to authorize the Consumer's access to some
	/// protected resource(s).
	/// </summary>
	[Serializable]
	public class UserAuthorizationRequest : MessageBase, ITokenContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationRequest"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		/// <param name="requestToken">The request token.</param>
		/// <param name="version">The OAuth version.</param>
		internal UserAuthorizationRequest(MessageReceivingEndpoint serviceProvider, string requestToken, Version version)
			: this(serviceProvider, version) {
			this.RequestToken = requestToken;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationRequest"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		/// <param name="version">The OAuth version.</param>
		internal UserAuthorizationRequest(MessageReceivingEndpoint serviceProvider, Version version)
			: base(MessageProtections.None, MessageTransport.Indirect, serviceProvider, version) {
		}

		/// <summary>
		/// Gets or sets the Request or Access Token.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This property IS accessible by a different name.")]
		string ITokenContainingMessage.Token {
			get { return this.RequestToken; }
			set { this.RequestToken = value; }
		}

		/// <summary>
		/// Gets the extra, non-OAuth parameters that will be included in the message.
		/// </summary>
		public new IDictionary<string, string> ExtraData {
			get { return base.ExtraData; }
		}

		/// <summary>
		/// Gets a value indicating whether this is a safe OAuth authorization request.
		/// </summary>
		/// <value><c>true</c> if the Consumer is using OAuth 1.0a or later; otherwise, <c>false</c>.</value>
		public bool IsUnsafeRequest {
			get { return this.Version < Protocol.V10a.Version; }
		}

		/// <summary>
		/// Gets the Request Token obtained in the previous step.
		/// </summary>
		/// <remarks>
		/// The Service Provider MAY declare this parameter as REQUIRED, or 
		/// accept requests to the User Authorization URL without it, in which 
		/// case it will prompt the User to enter it manually.
		/// </remarks>
		[MessagePart("oauth_token", IsRequired = false)]
		public string RequestToken { get; internal set; }

		/// <summary>
		/// Gets or sets a URL the Service Provider will use to redirect the User back 
		/// to the Consumer when Obtaining User Authorization is complete. Optional.
		/// </summary>
		[MessagePart("oauth_callback", IsRequired = false, MaxVersion = "1.0")]
		internal Uri Callback { get; set; }
	}
}
