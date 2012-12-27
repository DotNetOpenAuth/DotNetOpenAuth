//-----------------------------------------------------------------------
// <copyright file="ClientAuthorizationCompleteEventArgs.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Validation;

	/// <summary>
	/// Describes the results of a completed authorization flow.
	/// </summary>
	public class ClientAuthorizationCompleteEventArgs : EventArgs {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClientAuthorizationCompleteEventArgs"/> class.
		/// </summary>
		/// <param name="authorization">The authorization.</param>
		public ClientAuthorizationCompleteEventArgs(IAuthorizationState authorization) {
			Requires.NotNull(authorization, "authorization");
			this.Authorization = authorization;
		}

		/// <summary>
		/// Gets the authorization tracking object.
		/// </summary>
		/// <value>Null if authorization was rejected by the user.</value>
		public IAuthorizationState Authorization { get; private set; }
	}
}
