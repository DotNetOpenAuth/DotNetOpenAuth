using System;
using DotNetOAuth.Messaging;
using DotNetOAuth.OAuth;
using DotNetOAuth.OAuth.ChannelElements;

/// <summary>
/// Service Provider definitions.
/// </summary>
public static class Constants {
	public static Uri WebRootUrl { get; set; }

	public static ServiceProviderDescription SelfDescription {
		get {
			ServiceProviderDescription description = new ServiceProviderDescription {
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
		return new ServiceProvider(SelfDescription, Global.TokenManager);
	}
}
