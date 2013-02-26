//-----------------------------------------------------------------------
// <copyright file="YammerConsumer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	public static class YammerConsumer {
		/// <summary>
		/// The Consumer to use for accessing Google data APIs.
		/// </summary>
		public static readonly ServiceProviderDescription ServiceDescription =
			new ServiceProviderDescription(
				"https://www.yammer.com/oauth/request_token",
				"https://www.yammer.com/oauth/authorize",
				"https://www.yammer.com/oauth/access_token");

		/// <summary>
		/// Gets a value indicating whether the Twitter consumer key and secret are set in the web.config file.
		/// </summary>
		public static bool IsConsumerConfigured {
			get {
				return !string.IsNullOrEmpty(ConfigurationManager.AppSettings["yammerConsumerKey"]) &&
					!string.IsNullOrEmpty(ConfigurationManager.AppSettings["yammerConsumerSecret"]);
			}
		}

		public static Consumer CreateConsumer(bool forWeb = true) {
			string consumerKey = ConfigurationManager.AppSettings["yammerConsumerKey"];
			string consumerSecret = ConfigurationManager.AppSettings["yammerConsumerSecret"];
			if (IsConsumerConfigured) {
				ITemporaryCredentialStorage storage = forWeb ? (ITemporaryCredentialStorage)new CookieTemporaryCredentialStorage() : new MemoryTemporaryCredentialStorage();
				return new Consumer(consumerKey, consumerSecret, ServiceDescription, storage);
			} else {
				throw new InvalidOperationException("No Yammer OAuth consumer key and secret could be found in web.config AppSettings.");
			}
		}
	}
}
