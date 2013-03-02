namespace OAuthServiceProvider.Code {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// Service Provider definitions.
	/// </summary>
	public static class Constants {
		public static Uri WebRootUrl { get; set; }

		public static ServiceProviderHostDescription SelfDescription {
			get {
				var description = new ServiceProviderHostDescription {
					AccessTokenEndpoint = new MessageReceivingEndpoint(new Uri(WebRootUrl, "/OAuth.ashx"), HttpDeliveryMethods.PostRequest),
					RequestTokenEndpoint = new MessageReceivingEndpoint(new Uri(WebRootUrl, "/OAuth.ashx"), HttpDeliveryMethods.PostRequest),
					UserAuthorizationEndpoint = new MessageReceivingEndpoint(new Uri(WebRootUrl, "/OAuth.ashx"), HttpDeliveryMethods.PostRequest),
					TamperProtectionElements = new ITamperProtectionChannelBindingElement[] {
					new HmacSha1SigningBindingElement(),
				},
				};

				return description;
			}
		}

		public static ServiceProvider CreateServiceProvider() {
			return new ServiceProvider(SelfDescription, Global.TokenManager, Global.NonceStore);
		}
	}
}