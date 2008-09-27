//-----------------------------------------------------------------------
// <copyright file="AppendixScenarios.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test {
	using System;
	using System.IO;
	using System.Linq;
	using System.Net;
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
			ServiceProviderEndpoint accessPhotoEndpoint = new ServiceProviderEndpoint("http://photos.example.net/photos?file=vacation.jpg&size=original", HttpDeliveryMethod.AuthorizationHeaderRequest);
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
					string accessToken = consumer.ProcessUserAuthorization();
					var photoRequest = consumer.CreateAuthorizedRequestInternal(accessPhotoEndpoint, accessToken);
					Response protectedPhoto = channel.RequestProtectedResource(photoRequest);
					Assert.IsNotNull(protectedPhoto);
					Assert.AreEqual(HttpStatusCode.OK, protectedPhoto.Status);
					Assert.AreEqual("image/jpeg", protectedPhoto.Headers[HttpResponseHeader.ContentType]);
					Assert.AreNotEqual(0, protectedPhoto.ResponseStream.Length);
				},
				channel => {
					tokenManager.AddConsumer(consumer.ConsumerKey, consumer.ConsumerSecret);
					sp.Channel = channel;
					var requestTokenMessage = sp.ReadTokenRequest();
					sp.SendUnauthorizedTokenResponse(requestTokenMessage);
					var authRequest = sp.ReadAuthorizationRequest();
					tokenManager.AuthorizeRequestToken(authRequest.RequestToken);
					sp.SendAuthorizationResponse(authRequest);
					var accessRequest = sp.ReadAccessTokenRequest();
					sp.SendAccessToken(accessRequest);
					string accessToken = sp.GetAccessTokenInRequest();
					channel.SendDirectRawResponse(new Response {
						ResponseStream = new MemoryStream(new byte[] { 0x33, 0x66 }),
						Headers = new WebHeaderCollection {
									{ HttpResponseHeader.ContentType, "image/jpeg" },
								},
					});
				});
			coordinator.SigningElement = (ITamperProtectionChannelBindingElement)sp.Channel.BindingElements.Single(el => el is ITamperProtectionChannelBindingElement);
			coordinator.Run();
		}
	}
}
