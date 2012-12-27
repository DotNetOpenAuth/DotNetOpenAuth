//-----------------------------------------------------------------------
// <copyright file="ExpiredMessageException.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;
	using System.Globalization;
	using Validation;

	/// <summary>
	/// An exception thrown when a message is received that exceeds the maximum message age limit.
	/// </summary>
	[Serializable]
	internal class ExpiredMessageException : ProtocolException {
		/// <summary>
		/// Initializes a new instance of the <see cref="ExpiredMessageException"/> class.
		/// </summary>
		/// <param name="utcExpirationDate">The date the message expired.</param>
		/// <param name="faultedMessage">The expired message.</param>
		public ExpiredMessageException(DateTime utcExpirationDate, IProtocolMessage faultedMessage)
			: base(string.Format(CultureInfo.CurrentCulture, MessagingStrings.ExpiredMessage, utcExpirationDate.ToLocalTime(), DateTime.Now), faultedMessage) {
			Requires.Argument(utcExpirationDate.Kind == DateTimeKind.Utc, "utcExpirationDate", "Time must be expressed as UTC.");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpiredMessageException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> 
		/// that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The System.Runtime.Serialization.StreamingContext 
		/// that contains contextual information about the source or destination.</param>
		protected ExpiredMessageException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
