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
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	public class YammerConsumer : Consumer {
		/// <summary>
		/// The Consumer to use for accessing Google data APIs.
		/// </summary>
		public static readonly ServiceProviderDescription ServiceDescription =
			new ServiceProviderDescription(
				"https://www.yammer.com/oauth/request_token",
				"https://www.yammer.com/oauth/authorize",
				"https://www.yammer.com/oauth/access_token");

		public YammerConsumer() {
			this.ServiceProvider = ServiceDescription;
			this.ConsumerKey = ConfigurationManager.AppSettings["YammerConsumerKey"];
			this.ConsumerSecret = ConfigurationManager.AppSettings["YammerConsumerSecret"];
			this.TemporaryCredentialStorage = HttpContext.Current != null
				                                  ? (ITemporaryCredentialStorage)new CookieTemporaryCredentialStorage()
				                                  : new MemoryTemporaryCredentialStorage();
		}

		/// <summary>
		/// Gets a value indicating whether the Twitter consumer key and secret are set in the web.config file.
		/// </summary>
		public static bool IsConsumerConfigured {
			get {
				return !string.IsNullOrEmpty(ConfigurationManager.AppSettings["yammerConsumerKey"]) &&
					!string.IsNullOrEmpty(ConfigurationManager.AppSettings["yammerConsumerSecret"]);
			}
		}
	}
}
