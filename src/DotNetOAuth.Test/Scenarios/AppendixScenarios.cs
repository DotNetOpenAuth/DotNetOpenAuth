//-----------------------------------------------------------------------
// <copyright file="AppendixScenarios.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test {
	using System;
	using System.Linq;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Test.Mocks;
	using DotNetOAuth.Test.Scenarios;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AppendixScenarios : TestBase {
		[TestMethod]
		public void SpecAppendixAExample() {
			ServiceProviderEndpoints endpoints = new ServiceProviderEndpoints() {
				RequestTokenEndpoint = new ServiceProviderEndpoint("https://photos.example.net/request_token", HttpDeliveryMethod.PostRequest),
				UserAuthorizationEndpoint = new ServiceProviderEndpoint("http://photos.example.net/authorize", HttpDeliveryMethod.GetRequest),
				AccessTokenEndpoint = new ServiceProviderEndpoint("https://photos.example.net/access_token", HttpDeliveryMethod.PostRequest),
			};
			var tokenManager = new InMemoryTokenManager();
			var sp = new ServiceProvider(endpoints, tokenManager);
			Consumer consumer = new Consumer(endpoints, new InMemoryTokenManager()) {
				ConsumerKey = "dpf43f3p2l4k3l03",
				ConsumerSecret = "kd94hf93k423kf44",
			};

			Coordinator coordinator = new Coordinator(
				channel => {
					consumer.Channel = channel;
					consumer.RequestUserAuthorization(new Uri("http://printer.example.com/request_token_ready"));
					var accessTokenMessage = consumer.ProcessUserAuthorization();
				},
				channel => {
					tokenManager.AddConsumer(consumer.ConsumerKey, consumer.ConsumerSecret);
					sp.Channel = channel;
					var requestTokenMessage = sp.ReadTokenRequest();
					sp.SendUnauthorizedTokenResponse(requestTokenMessage);
					var authRequest = sp.ReadAuthorizationRequest();
					tokenManager.AuthorizedRequestToken(authRequest.RequestToken);
					sp.SendAuthorizationResponse(authRequest);
					var accessRequest = sp.ReadAccessTokenRequest();
					sp.SendAccessToken(accessRequest);
				});
			coordinator.SigningElement = (SigningBindingElementBase)sp.Channel.BindingElements.Single(el => el is SigningBindingElementBase);
			coordinator.Start();
		}
	}
}
