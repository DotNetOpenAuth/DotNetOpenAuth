namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal interface IRequestWithRedirectUri {
		string ClientIdentifier { get; }

		Uri Callback { get; }
	}
}
