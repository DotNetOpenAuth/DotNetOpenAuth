//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
	public class UserAuthorizationRequest : MessageBase, ITokenContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationRequest"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		/// <param name="requestToken">The request token.</param>
		internal UserAuthorizationRequest(MessageReceivingEndpoint serviceProvider, string requestToken)
			: this(serviceProvider) {
			this.RequestToken = requestToken;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationRequest"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		internal UserAuthorizationRequest(MessageReceivingEndpoint serviceProvider)
			: base(MessageProtections.None, MessageTransport.Indirect, serviceProvider) {
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
		/// Gets or sets the Request Token obtained in the previous step.
		/// </summary>
		/// <remarks>
		/// The Service Provider MAY declare this parameter as REQUIRED, or 
		/// accept requests to the User Authorization URL without it, in which 
		/// case it will prompt the User to enter it manually.
		/// </remarks>
		[MessagePart("oauth_token", IsRequired = false)]
		internal string RequestToken { get; set; }

		/// <summary>
		/// Gets or sets a URL the Service Provider will use to redirect the User back 
		/// to the Consumer when Obtaining User Authorization is complete. Optional.
		/// </summary>
		[MessagePart("oauth_callback", IsRequired = false)]
		internal Uri Callback { get; set; }
	}
}
