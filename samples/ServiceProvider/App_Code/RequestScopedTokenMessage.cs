using DotNetOAuth.Messages;
using DotNetOAuth.Messaging;

/// <summary>
/// A custom web app version of the message sent to request an unauthorized token.
/// </summary>
public class RequestScopedTokenMessage : GetRequestTokenMessage {
	/// <summary>
	/// Initializes a new instance of the <see cref="RequestScopedTokenMessage"/> class.
	/// </summary>
	/// <param name="endpoint">The endpoint that will receive the message.</param>
	public RequestScopedTokenMessage(MessageReceivingEndpoint endpoint) : base(endpoint) {
	}

	/// <summary>
	/// Gets or sets the scope of the access being requested.
	/// </summary>
	[MessagePart("scope", IsRequired = true)]
	public string Scope { get; set; }
}
