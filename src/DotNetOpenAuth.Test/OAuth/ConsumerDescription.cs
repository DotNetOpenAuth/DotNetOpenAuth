//-----------------------------------------------------------------------
// <copyright file="ConsumerDescription.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth {
	using DotNetOpenAuth.OAuth;

	/// <summary>
	/// Information necessary to initialize a <see cref="Consumer"/>,
	/// and to tell a <see cref="ServiceProvider"/> about it.
	/// </summary>
	/// <remarks>
	/// This type is immutable.
	/// </remarks>
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
		/// Gets the consumer key.
		/// </summary>
		/// <value>The consumer key.</value>
		internal string ConsumerKey { get; private set; }

		/// <summary>
		/// Gets the consumer secret.
		/// </summary>
		/// <value>The consumer secret.</value>
		internal string ConsumerSecret { get; private set; }
	}
}
