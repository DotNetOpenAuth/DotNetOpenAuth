//-----------------------------------------------------------------------
// <copyright file="AutomatedAuthorizationCheckResponse.cs" company="Andrew Arnott">
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
	/// Describes the result of an automated authorization check, such as for client credential or resource owner password grants.
	/// </summary>
	public class AutomatedAuthorizationCheckResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="AutomatedAuthorizationCheckResponse" /> class.
		/// </summary>
		/// <param name="accessRequest">The access token request.</param>
		/// <param name="approved">A value indicating whether the authorization should be approved.</param>
		public AutomatedAuthorizationCheckResponse(IAccessTokenRequest accessRequest, bool approved) {
			Requires.NotNull(accessRequest, "accessRequest");

			this.IsApproved = approved;
			this.ApprovedScope = new HashSet<string>(accessRequest.Scope);
		}

		/// <summary>
		/// Gets a value indicating whether the authorization should be approved.
		/// </summary>
		public bool IsApproved { get; private set; }

		/// <summary>
		/// Gets the scope to be granted.
		/// </summary>
		public HashSet<string> ApprovedScope { get; private set; }
	}
}
