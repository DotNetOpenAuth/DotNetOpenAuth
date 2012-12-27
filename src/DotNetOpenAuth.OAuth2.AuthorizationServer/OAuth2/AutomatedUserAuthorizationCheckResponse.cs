//-----------------------------------------------------------------------
// <copyright file="AutomatedUserAuthorizationCheckResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using DotNetOpenAuth.OAuth2.Messages;
	using Validation;

	/// <summary>
	/// Describes the result of an automated authorization check for resource owner grants.
	/// </summary>
	public class AutomatedUserAuthorizationCheckResponse : AutomatedAuthorizationCheckResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="AutomatedUserAuthorizationCheckResponse" /> class.
		/// </summary>
		/// <param name="accessRequest">The access token request.</param>
		/// <param name="approved">A value indicating whether the authorization should be approved.</param>
		/// <param name="canonicalUserName">
		/// Canonical username of the authorizing user (resource owner), as the resource server would recognize it.
		/// Ignored if <paramref name="approved"/> is false.
		/// </param>
		public AutomatedUserAuthorizationCheckResponse(IAccessTokenRequest accessRequest, bool approved, string canonicalUserName)
			: base(accessRequest, approved) {
			if (approved) {
				Requires.NotNullOrEmpty(canonicalUserName, "canonicalUserName");
			}

			this.CanonicalUserName = canonicalUserName;
		}

		/// <summary>
		/// Gets the canonical username of the authorizing user (resource owner).
		/// </summary>
		public string CanonicalUserName { get; private set; }
	}
}
