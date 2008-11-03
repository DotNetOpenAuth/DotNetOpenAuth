//-----------------------------------------------------------------------
// <copyright file="CommonConsumerBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.CommonConsumers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.ChannelElements;

	/// <summary>
	/// A useful base class to derive from for Consumers written against a specific Service Provider.
	/// </summary>
	public abstract class CommonConsumerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="CommonConsumerBase"/> class.
		/// </summary>
		/// <param name="serviceDescription">The service description.</param>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="consumerKey">The consumer key.</param>
		protected CommonConsumerBase(ServiceProviderDescription serviceDescription, ITokenManager tokenManager, string consumerKey) {
			if (serviceDescription == null) {
				throw new ArgumentNullException("serviceDescription");
			}
			if (tokenManager == null) {
				throw new ArgumentNullException("tokenManager");
			}
			if (consumerKey == null) {
				throw new ArgumentNullException("consumerKey");
			}
			this.Consumer = new WebConsumer(serviceDescription, tokenManager) {
				ConsumerKey = consumerKey,
			};
		}

		/// <summary>
		/// Gets the consumer.
		/// </summary>
		protected WebConsumer Consumer { get; private set; }

		/// <summary>
		/// Enumerates through the individual set bits in a flag enum.
		/// </summary>
		/// <param name="flags">The flags enum value.</param>
		/// <returns>An enumeration of just the <i>set</i> bits in the flags enum.</returns>
		protected static IEnumerable<long> GetIndividualFlags(Enum flags) {
			long flagsLong = Convert.ToInt64(flags);
			for (int i = 0; i < sizeof(long) * 8; i++) { // long is the type behind the largest enum
				// Select an individual application from the scopes.
				long individualFlagPosition = (long)Math.Pow(2, i);
				long individualFlag = flagsLong & individualFlagPosition;
				if (individualFlag == individualFlagPosition) {
					yield return individualFlag;
				}
			}
		}
	}
}
