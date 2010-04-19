namespace OAuthServiceProvider.Code {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// A custom web app version of the message sent to request an unauthorized token.
	/// </summary>
	public class RequestScopedTokenMessage : UnauthorizedTokenRequest {
		/// <summary>
		/// Initializes a new instance of the <see cref="RequestScopedTokenMessage"/> class.
		/// </summary>
		/// <param name="endpoint">The endpoint that will receive the message.</param>
		/// <param name="version">The OAuth version.</param>
		public RequestScopedTokenMessage(MessageReceivingEndpoint endpoint, Version version)
			: base(endpoint, version) {
		}

		/// <summary>
		/// Gets or sets the scope of the access being requested.
		/// </summary>
		[MessagePart("scope", IsRequired = true)]
		public string Scope { get; set; }
	}
}