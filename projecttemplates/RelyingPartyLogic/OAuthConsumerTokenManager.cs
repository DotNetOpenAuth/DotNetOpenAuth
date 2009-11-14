//-----------------------------------------------------------------------
// <copyright file="OAuthConsumerTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;

	public class OAuthConsumerTokenManager : OAuthTokenManager, IConsumerTokenManager {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthConsumerTokenManager"/> class.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		public OAuthConsumerTokenManager(string consumerKey, string consumerSecret) {
			if (String.IsNullOrEmpty(consumerKey)) {
				throw new ArgumentNullException("consumerKey");
			}
			if (consumerSecret == null) {
				throw new ArgumentNullException("consumerSecret");
			}

			this.ConsumerKey = consumerKey;
			this.ConsumerSecret = consumerSecret;
		}

		#region IConsumerTokenManager Members

		/// <summary>
		/// Gets the consumer key.
		/// </summary>
		/// <value>The consumer key.</value>
		public string ConsumerKey { get; private set; }

		/// <summary>
		/// Gets the consumer secret.
		/// </summary>
		/// <value>The consumer secret.</value>
		public string ConsumerSecret { get; private set; }

		#endregion
	}
}
