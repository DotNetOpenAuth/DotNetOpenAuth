//-----------------------------------------------------------------------
// <copyright file="AccessTokenResourceOwnerPasswordCredentialsRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
	/// A request from a Client to an Authorization Server to exchange the user's username and password for an access token.
	/// </summary>
	internal class AccessTokenResourceOwnerPasswordCredentialsRequest : ScopedAccessTokenRequest, IAuthorizationCarryingRequest, IAuthorizationDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenResourceOwnerPasswordCredentialsRequest"/> class.
		/// </summary>
		/// <param name="accessTokenEndpoint">The access token endpoint.</param>
		/// <param name="version">The protocol version.</param>
		internal AccessTokenResourceOwnerPasswordCredentialsRequest(Uri accessTokenEndpoint, Version version)
			: base(accessTokenEndpoint, version) {
		}

		#region IAuthorizationCarryingRequest members

		/// <summary>
		/// Gets the authorization that the code or token describes.
		/// </summary>
		IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription {
			get { return this.CredentialsValidated ? this : null; }
		}

		#endregion

		#region IAuthorizationDescription Members

		/// <summary>
		/// Gets the date this authorization was established or the token was issued.
		/// </summary>
		/// <value>A date/time expressed in UTC.</value>
		DateTime IAuthorizationDescription.UtcIssued {
			get { return DateTime.UtcNow; }
		}

		/// <summary>
		/// Gets the name on the account whose data on the resource server is accessible using this authorization.
		/// </summary>
		string IAuthorizationDescription.User {
			get { return this.RequestingUserName; }
		}

		/// <summary>
		/// Gets the scope of operations the client is allowed to invoke.
		/// </summary>
		HashSet<string> IAuthorizationDescription.Scope {
			get { return this.Scope; }
		}

		#endregion

		/// <summary>
		/// Gets the username of the authorizing user, when applicable.
		/// </summary>
		/// <value>
		/// A non-empty string; or <c>null</c> when no user has authorized this access token.
		/// </value>
		public override string UserName {
			get { return base.UserName ?? this.RequestingUserName; }
		}

		/// <summary>
		/// Gets the type of the grant.
		/// </summary>
		/// <value>The type of the grant.</value>
		internal override GrantType GrantType {
			get { return Messages.GrantType.Password; }
		}

		/// <summary>
		/// Gets or sets the user's account username.
		/// </summary>
		/// <value>The username on the user's account.</value>
		[MessagePart(Protocol.username, IsRequired = true)]
		internal string RequestingUserName { get; set; }

		/// <summary>
		/// Gets or sets the user's password.
		/// </summary>
		/// <value>The password.</value>
		[MessagePart(Protocol.password, IsRequired = true, IsSecuritySensitive = true)]
		internal string Password { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the resource owner's credentials have been validated at the authorization server.
		/// </summary>
		internal bool CredentialsValidated { get; set; }
	}
}
