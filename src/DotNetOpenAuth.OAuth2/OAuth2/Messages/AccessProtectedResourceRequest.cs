//-----------------------------------------------------------------------
// <copyright file="AccessProtectedResourceRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
	/// <remarks>
	/// In its current form, this class only accepts bearer access tokens. 
	/// When support for additional access token types is added, this class should probably be refactored
	/// into derived types, where each derived type supports a particular access token type.
	/// </remarks>
	internal class AccessProtectedResourceRequest : MessageBase, IAuthorizationCarryingRequest {
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
		CodeOrTokenType IAuthorizationCarryingRequest.CodeOrTokenType {
			get { return CodeOrTokenType.AccessToken; }
		}

		/// <summary>
		/// Gets or sets the verification code or refresh/access token.
		/// </summary>
		/// <value>The code or token.</value>
		string IAuthorizationCarryingRequest.CodeOrToken {
			get { return this.AccessToken; }
			set { this.AccessToken = value; }
		}

		/// <summary>
		/// Gets or sets the authorization that the token describes.
		/// </summary>
		IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription { get; set; }

		/// <summary>
		/// Gets the type of the access token.
		/// </summary>
		/// <value>
		/// Always "bearer".
		/// </value>
		[MessagePart("token_type", IsRequired = true)]
		internal string TokenType {
			get { return Protocol.AccessTokenTypes.Bearer; }
		}

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>The access token.</value>
		[MessagePart("access_token", IsRequired = true)]
		internal string AccessToken { get; set; }
	}
}
