//-----------------------------------------------------------------------
// <copyright file="IConsumerTokenManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	/// <summary>
	/// A token manager for use by a web site in its role as a consumer of
	/// an individual ServiceProvider.
	/// </summary>
	public interface IConsumerTokenManager : ITokenManager {
		/// <summary>
		/// Gets the consumer key.
		/// </summary>
		/// <value>The consumer key.</value>
		string ConsumerKey { get; }

		/// <summary>
		/// Gets the consumer secret.
		/// </summary>
		/// <value>The consumer secret.</value>
		string ConsumerSecret { get; }
	}
}
