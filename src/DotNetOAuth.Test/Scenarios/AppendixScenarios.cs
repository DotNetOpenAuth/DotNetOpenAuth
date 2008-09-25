//-----------------------------------------------------------------------
// <copyright file="Scenarios.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test {
	using System;
	using System.Collections.Specialized;
	using System.IO;
	using System.Net;
	using System.Web;
	using DotNetOAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOAuth.Test.Scenarios;
	using DotNetOAuth.ChannelElements;

	[TestClass]
	public class AppendixScenarios : TestBase {
		[TestMethod]
		public void SpecAppendixAExample() {
			ServiceProvider sp = new ServiceProvider {
				RequestTokenEndpoint = new ServiceProviderEndpoint("https://photos.example.net/request_token", HttpDeliveryMethod.PostRequest),
				UserAuthorizationEndpoint = new ServiceProviderEndpoint("http://photos.example.net/authorize", HttpDeliveryMethod.GetRequest),
				AccessTokenEndpoint = new ServiceProviderEndpoint("https://photos.example.net/access_token", HttpDeliveryMethod.PostRequest),
			};

			Coordinator coordinator = new Coordinator(
				channel => {
					Consumer consumer = new Consumer {
						Channel = channel,
						ConsumerKey = "dpf43f3p2l4k3l03",
						ConsumerSecret = "kd94hf93k423kf44",
						ServiceProvider = sp,
					};

					string requestTokenSecret = consumer.RequestUserAuthorization(new Uri("http://printer.example.com/request_token_ready"));
					var accessTokenMessage = consumer.ProcessUserAuthorization(requestTokenSecret);
				},
				channel => {
					sp.Channel = channel;
					var requestTokenMessage = sp.ReadTokenRequest();
					sp.SendUnauthorizedTokenResponse("hh5s93j4hdidpola", "hdhd0244k9j7ao03");
					var authRequest = sp.ReadAuthorizationRequest();
					sp.SendAuthorizationResponse(authRequest);
					var accessRequest = sp.ReadAccessTokenRequest();
					sp.SendAccessToken("nnch734d00sl2jdk", "pfkkdhi9sl3r4s00");
				});
			coordinator.SigningElement = new PlainTextSigningBindingElement();
			coordinator.Start();
		}
	}
}
