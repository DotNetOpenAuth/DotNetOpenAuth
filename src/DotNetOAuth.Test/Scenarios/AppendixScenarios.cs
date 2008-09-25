//-----------------------------------------------------------------------
// <copyright file="Scenarios.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test {
	using System;
	using System.Collections.Specialized;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Web;
	using DotNetOAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOAuth.Test.Scenarios;
	using DotNetOAuth.ChannelElements;
	using System.Collections.Generic;
	using DotNetOAuth.Test.Mocks;

	[TestClass]
	public class AppendixScenarios : TestBase {
		[TestMethod]
		public void SpecAppendixAExample() {
			ServiceProviderEndpoints spEndpoints = new ServiceProviderEndpoints() {
				RequestTokenEndpoint = new ServiceProviderEndpoint("https://photos.example.net/request_token", HttpDeliveryMethod.PostRequest),
				UserAuthorizationEndpoint = new ServiceProviderEndpoint("http://photos.example.net/authorize", HttpDeliveryMethod.GetRequest),
				AccessTokenEndpoint = new ServiceProviderEndpoint("https://photos.example.net/access_token", HttpDeliveryMethod.PostRequest),
			};
			var tokenManager = new InMemoryTokenManager();
			var sp = new ServiceProvider(spEndpoints, tokenManager);
			Consumer consumer = new Consumer {
				ConsumerKey = "dpf43f3p2l4k3l03",
				ConsumerSecret = "kd94hf93k423kf44",
				ServiceProvider = spEndpoints,
			};

			Coordinator coordinator = new Coordinator(
				channel => {
					consumer.Channel = channel;
					string requestTokenSecret = consumer.RequestUserAuthorization(new Uri("http://printer.example.com/request_token_ready"));
					var accessTokenMessage = consumer.ProcessUserAuthorization(requestTokenSecret);
				},
				channel => {
					tokenManager.AddConsumer(consumer.ConsumerKey, consumer.ConsumerSecret);
					sp.Channel = channel;
					var requestTokenMessage = sp.ReadTokenRequest();
					sp.SendUnauthorizedTokenResponse(requestTokenMessage);
					var authRequest = sp.ReadAuthorizationRequest();
					sp.SendAuthorizationResponse(authRequest);
					var accessRequest = sp.ReadAccessTokenRequest();
					sp.SendAccessToken(accessRequest);
				});
			coordinator.SigningElement = (SigningBindingElementBase)sp.Channel.BindingElements.Single(el => el is SigningBindingElementBase);
			coordinator.Start();
		}
	}
}
