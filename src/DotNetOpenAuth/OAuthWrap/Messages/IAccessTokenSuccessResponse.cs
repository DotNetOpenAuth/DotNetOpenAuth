namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal interface IAccessTokenSuccessResponse {
		string AccessToken { get; }

		string AccessTokenSecret { get; }

		string RefreshToken { get; }

		TimeSpan? Lifetime { get; }

		string Scope { get; }
	}
}
