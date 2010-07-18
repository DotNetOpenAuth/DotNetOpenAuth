//-----------------------------------------------------------------------
// <copyright file="IConsumerDescription.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;

	/// <summary>
	/// A description of a client from an Authorization Server's point of view.
	/// </summary>
	public interface IConsumerDescription {
		/// <summary>
		/// Gets the client secret.
		/// </summary>
		string Secret { get; }

		/// <summary>
		/// Gets the callback URI that this client has pre-registered with the service provider, if any.
		/// </summary>
		/// <value>A URI that user authorization responses should be directed to; or <c>null</c> if no preregistered callback was arranged.</value>
		Uri Callback { get; }
	}
}
