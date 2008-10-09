using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOAuth.ChannelElements;
using DotNetOAuth.Messages;

/// <summary>
/// A custom class that will cause the OAuth library to use our custom message types
/// where we have them.
/// </summary>
public class CustomOAuthTypeProvider : OAuthServiceProviderMessageTypeProvider {
	/// <summary>
	/// Initializes a new instance of the <see cref="CustomOAuthTypeProvider"/> class.
	/// </summary>
	/// <param name="tokenManager">The token manager instance to use.</param>
	public CustomOAuthTypeProvider(ITokenManager tokenManager) : base(tokenManager) {
	}

	public override Type GetRequestMessageType(IDictionary<string, string> fields) {
		Type type = base.GetRequestMessageType(fields);

		// inject our own type here to replace the standard one
		if (type == typeof(GetRequestTokenMessage)) {
			type = typeof(RequestScopedTokenMessage);
		}

		return type;
	}
}
