//-----------------------------------------------------------------------
// <copyright file="DirectUserToServiceProviderMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A message used to redirect the user from a Consumer to a Service Provider's web site.
	/// </summary>
	internal class DirectUserToServiceProviderMessage : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectUserToServiceProviderMessage"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		internal DirectUserToServiceProviderMessage(MessageReceivingEndpoint serviceProvider)
			: base(MessageProtection.None, MessageTransport.Indirect, serviceProvider) {
		}

		/// <summary>
		/// Gets or sets the Request Token obtained in the previous step.
		/// </summary>
		/// <remarks>
		/// The Service Provider MAY declare this parameter as REQUIRED, or 
		/// accept requests to the User Authorization URL without it, in which 
		/// case it will prompt the User to enter it manually.
		/// </remarks>
		[MessagePart(Name = "oauth_token", IsRequired = false)]
		public string RequestToken { get; set; }

		/// <summary>
		/// Gets or sets a URL the Service Provider will use to redirect the User back 
		/// to the Consumer when Obtaining User Authorization is complete. Optional.
		/// </summary>
		[MessagePart(Name = "oauth_callback", IsRequired = false)]
		public Uri Callback { get; set; }
	}
}
