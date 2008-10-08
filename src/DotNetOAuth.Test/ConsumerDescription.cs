//-----------------------------------------------------------------------
// <copyright file="ConsumerDescription.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test {
	/// <summary>
	/// Information necessary to initialize a <see cref="Consumer"/>,
	/// and to tell a <see cref="ServiceProvider"/> about it.
	/// </summary>
	internal class ConsumerDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="ConsumerDescription"/> class.
		/// </summary>
		/// <param name="key">The consumer key.</param>
		/// <param name="secret">The consumer secret.</param>
		internal ConsumerDescription(string key, string secret) {
			this.ConsumerKey = key;
			this.ConsumerSecret = secret;
		}

		/// <summary>
		/// Gets or sets the consumer key.
		/// </summary>
		/// <value>The consumer key.</value>
		internal string ConsumerKey { get; set; }

		/// <summary>
		/// Gets or sets the consumer secret.
		/// </summary>
		/// <value>The consumer secret.</value>
		internal string ConsumerSecret { get; set; }
	}
}
