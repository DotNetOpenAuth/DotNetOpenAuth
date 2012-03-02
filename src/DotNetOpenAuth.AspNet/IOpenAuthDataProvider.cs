//-----------------------------------------------------------------------
// <copyright file="IOpenAuthDataProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet {
	/// <summary>
	/// The i open auth data provider.
	/// </summary>
	public interface IOpenAuthDataProvider {
		#region Public Methods and Operators

		/// <summary>
		/// The get user name from open auth.
		/// </summary>
		/// <param name="openAuthProvider">
		/// The open auth provider.
		/// </param>
		/// <param name="openAuthId">
		/// The open auth id.
		/// </param>
		/// <returns>
		/// The get user name from open auth.
		/// </returns>
		string GetUserNameFromOpenAuth(string openAuthProvider, string openAuthId);
		#endregion
	}
}
