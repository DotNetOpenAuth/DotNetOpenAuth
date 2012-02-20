//-----------------------------------------------------------------------
// <copyright file="IMessageWithEvents.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	/// <summary>
	/// An interface that messages wishing to perform custom serialization/deserialization
	/// may implement to be notified of <see cref="Channel"/> events.
	/// </summary>
	internal interface IMessageWithEvents : IMessage {
		/// <summary>
		/// Called when the message is about to be transmitted,
		/// before it passes through the channel binding elements.
		/// </summary>
		void OnSending();

		/// <summary>
		/// Called when the message has been received, 
		/// after it passes through the channel binding elements.
		/// </summary>
		void OnReceiving();
	}
}
