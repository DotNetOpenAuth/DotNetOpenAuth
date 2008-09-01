namespace DotNetOAuth {
	using System;

	/// <summary>
	/// Implemented by messages that are sent as requests.
	/// </summary>
	interface IProtocolMessageRequest : IProtocolMessage {
		/// <summary>
		/// The URL of the intended receiver of this message.
		/// </summary>
		Uri Recipient {
			get;
			set;
		}
	}
}
