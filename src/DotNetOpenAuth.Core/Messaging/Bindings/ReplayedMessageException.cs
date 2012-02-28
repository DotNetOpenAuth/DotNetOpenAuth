//-----------------------------------------------------------------------
// <copyright file="ReplayedMessageException.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;

	/// <summary>
	/// An exception thrown when a message is received for the second time, signalling a possible
	/// replay attack.
	/// </summary>
	[Serializable]
	internal class ReplayedMessageException : ProtocolException {
		/// <summary>
		/// Initializes a new instance of the <see cref="ReplayedMessageException"/> class.
		/// </summary>
		/// <param name="faultedMessage">The replayed message.</param>
		public ReplayedMessageException(IProtocolMessage faultedMessage) : base(MessagingStrings.ReplayAttackDetected, faultedMessage) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ReplayedMessageException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> 
		/// that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The System.Runtime.Serialization.StreamingContext 
		/// that contains contextual information about the source or destination.</param>
		protected ReplayedMessageException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
