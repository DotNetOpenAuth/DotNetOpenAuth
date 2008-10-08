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
			ServiceProviderDescription serviceDescription = new ServiceProviderDescription() {
				RequestTokenEndpoint = new MessageReceivingEndpoint("https://photos.example.net/request_token", HttpDeliveryMethod.PostRequest),
				UserAuthorizationEndpoint = new MessageReceivingEndpoint("http://photos.example.net/authorize", HttpDeliveryMethod.GetRequest),
				AccessTokenEndpoint = new MessageReceivingEndpoint("https://photos.example.net/access_token", HttpDeliveryMethod.PostRequest),
				TamperProtectionElements = new ITamperProtectionChannelBindingElement[] {
					new PlainTextSigningBindingElement(),
					new HmacSha1SigningBindingElement(),
				},
			};
			MessageReceivingEndpoint accessPhotoEndpoint = new MessageReceivingEndpoint("http://photos.example.net/photos?file=vacation.jpg&size=original", HttpDeliveryMethod.AuthorizationHeaderRequest | HttpDeliveryMethod.GetRequest);
			string consumerKey = "dpf43f3p2l4k3l03";
			string consumerSecret = "kd94hf93k423kf44";

			Coordinator coordinator = new Coordinator(
				serviceDescription,
				consumer => {
					consumer.ConsumerKey = consumerKey;
					consumer.ConsumerSecret = consumerSecret;
					consumer.RequestUserAuthorization(new Uri("http://printer.example.com/request_token_ready"), null, null);
					string accessToken = consumer.ProcessUserAuthorization().AccessToken;
					var photoRequest = consumer.CreateAuthorizedRequestInternal(accessPhotoEndpoint, accessToken);
					Response protectedPhoto = ((CoordinatingOAuthChannel)consumer.Channel).RequestProtectedResource(photoRequest);
					Assert.IsNotNull(protectedPhoto);
					Assert.AreEqual(HttpStatusCode.OK, protectedPhoto.Status);
					Assert.AreEqual("image/jpeg", protectedPhoto.Headers[HttpResponseHeader.ContentType]);
					Assert.AreNotEqual(0, protectedPhoto.ResponseStream.Length);
				},
				sp => {
					((InMemoryTokenManager)sp.TokenManager).AddConsumer(consumerKey, consumerSecret);
					var requestTokenMessage = sp.ReadTokenRequest();
					sp.SendUnauthorizedTokenResponse(requestTokenMessage, null);
					var authRequest = sp.ReadAuthorizationRequest();
					((InMemoryTokenManager)sp.TokenManager).AuthorizeRequestToken(authRequest.RequestToken);
					sp.SendAuthorizationResponse(authRequest);
					var accessRequest = sp.ReadAccessTokenRequest();
					sp.SendAccessToken(accessRequest, null);
					string accessToken = sp.GetProtectedResourceAuthorization().AccessToken;
					((CoordinatingOAuthChannel)sp.Channel).SendDirectRawResponse(new Response {
						ResponseStream = new MemoryStream(new byte[] { 0x33, 0x66 }),
						Headers = new WebHeaderCollection {
							{ HttpResponseHeader.ContentType, "image/jpeg" },
						},
					});
				});

			coordinator.Run();
		}
	}
}
