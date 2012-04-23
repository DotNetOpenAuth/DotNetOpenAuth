//-----------------------------------------------------------------------
// <copyright file="AccessTokenClientCredentialsRequest.cs" company="Outercurve Foundation">
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
	/// A request for an access token for a client application that has its
	/// own (non-user affiliated) client name and password.
	/// </summary>
	/// <remarks>
	/// This is somewhat analogous to 2-legged OAuth.
	/// </remarks>
	internal class AccessTokenClientCredentialsRequest : ScopedAccessTokenRequest, IAuthorizationCarryingRequest, IAuthorizationDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenClientCredentialsRequest"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal AccessTokenClientCredentialsRequest(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		#region IAuthorizationCarryingRequest members

		/// <summary>
		/// Gets the authorization that the code or token describes.
		/// </summary>
		IAuthorizationDescription IAuthorizationCarryingRequest.AuthorizationDescription {
			get { return this.ClientAuthenticated ? this : null; }
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
			get { return null; }
		}

		/// <summary>
		/// Gets the scope of operations the client is allowed to invoke.
		/// </summary>
		HashSet<string> IAuthorizationDescription.Scope {
			get { return this.Scope; }
		}

		#endregion

		/// <summary>
		/// Gets the type of the grant.
		/// </summary>
		/// <value>The type of the grant.</value>
		internal override GrantType GrantType {
			get { return Messages.GrantType.ClientCredentials; }
		}
	}
}
