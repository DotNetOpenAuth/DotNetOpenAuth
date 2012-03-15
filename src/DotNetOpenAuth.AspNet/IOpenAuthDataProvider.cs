//-----------------------------------------------------------------------
// <copyright file="IOpenAuthDataProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet {
	/// <summary>
	/// Common methods available on identity issuers.
	/// </summary>
	public interface IOpenAuthDataProvider {
		#region Public Methods and Operators

		/// <summary>
		/// Get a user name from an identity provider and their own assigned user ID.
		/// </summary>
		/// <param name="openAuthProvider">
		/// The identity provider.
		/// </param>
		/// <param name="openAuthId">
		/// The issuer's ID for the user.
		/// </param>
		/// <returns>
		/// The username of the user.
		/// </returns>
		string GetUserNameFromOpenAuth(string openAuthProvider, string openAuthId);

		#endregion
	}
}
