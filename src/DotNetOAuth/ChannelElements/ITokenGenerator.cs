//-----------------------------------------------------------------------
// <copyright file="ITokenGenerator.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal interface ITokenGenerator {
		string GenerateRequestToken(string consumerKey);
		string GenerateAccessToken(string consumerKey);
		string GenerateSecret();
	}
}
