//-----------------------------------------------------------------------
// <copyright file="IExpiringProtocolMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;

	/// <summary>
	/// The contract a message that has an allowable time window for processing must implement.
	/// </summary>
	/// <remarks>
	/// All expiring messages must also be signed to prevent tampering with the creation date.
	/// </remarks>
	internal interface IExpiringProtocolMessage : IProtocolMessage {
		/// <summary>
		/// Gets or sets the UTC date/time the message was originally sent onto the network.
		/// </summary>
		/// <remarks>
		/// The property setter should ensure a UTC date/time,
		/// and throw an exception if this is not possible.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown when a DateTime that cannot be converted to UTC is set.
		/// </exception>
		DateTime UtcCreationDate { get; set; }
	}
}
