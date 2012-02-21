//-----------------------------------------------------------------------
// <copyright file="OAuth2TestBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth2;

	public class OAuth2TestBase : TestBase {
		protected internal const string ClientId = "TestClientId";

		protected internal const string ClientSecret = "TestClientSecret";

		protected const string Username = "TestUser";

		protected static readonly Uri ClientCallback = new Uri("http://client/callback");

		protected static readonly AuthorizationServerDescription AuthorizationServerDescription = new AuthorizationServerDescription {
			AuthorizationEndpoint = new Uri("https://authserver/authorize"),
			TokenEndpoint = new Uri("https://authserver/token"),
		};

		protected static readonly IClientDescription ClientDescription = new ClientDescription(
			ClientSecret,
			ClientCallback,
			ClientType.Confidential);
	}
}
