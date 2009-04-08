//-----------------------------------------------------------------------
// <copyright file="IServiceProviderTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A token manager for use by a web site in its role as a
	/// service provider.
	/// </summary>
	public interface IServiceProviderTokenManager : ITokenManager {
		/// <summary>
		/// Gets the Consumer Secret for a given a Consumer Key.
		/// </summary>
		/// <param name="consumerKey">The Consumer Key.</param>
		/// <returns>The Consumer Secret.</returns>
		/// <exception cref="ArgumentException">Thrown if the consumer key cannot be found.</exception>
		/// <exception cref="InvalidOperationException">May be thrown if called when the signature algorithm does not require a consumer secret, such as when RSA-SHA1 is used.</exception>
		string GetConsumerSecret(string consumerKey);
	}
}
