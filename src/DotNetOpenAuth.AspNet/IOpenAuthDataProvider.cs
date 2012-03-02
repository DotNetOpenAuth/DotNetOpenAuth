//-----------------------------------------------------------------------
// <copyright file="IOpenAuthDataProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet {
	public interface IOpenAuthDataProvider {
		string GetUserNameFromOpenAuth(string openAuthProvider, string openAuthId);
	}
}