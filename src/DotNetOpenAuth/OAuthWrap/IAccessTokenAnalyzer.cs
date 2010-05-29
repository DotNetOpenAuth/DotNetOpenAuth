//-----------------------------------------------------------------------
// <copyright file="IAccessTokenAnalyzer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public interface IAccessTokenAnalyzer {
		bool TryValidateAccessToken(string accessToken, out string user, out string scope);
	}
}
