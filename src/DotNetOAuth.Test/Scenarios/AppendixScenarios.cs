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
				RequestTokenEndpoint = new MessageReceivingEndpoint("https://photos.example.net/request_token", HttpDeliveryMethods.PostRequest),
				UserAuthorizationEndpoint = new MessageReceivingEndpoint("http://photos.example.net/authorize", HttpDeliveryMethods.GetRequest),
				AccessTokenEndpoint = new MessageReceivingEndpoint("https://photos.example.net/access_token", HttpDeliveryMethods.PostRequest),
				TamperProtectionElements = new ITamperProtectionChannelBindingElement[] {
					new PlaintextSigningBindingElement(),
					new HmacSha1SigningBindingElement(),
				},
			};
			MessageReceivingEndpoint accessPhotoEndpoint = new MessageReceivingEndpoint("http://photos.example.net/photos?file=vacation.jpg&size=original", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest);
			ConsumerDescription consumerDescription = new ConsumerDescription("dpf43f3p2l4k3l03", "kd94hf93k423kf44");

			Coordinator coordinator = new Coordinator(
				consumerDescription,
				serviceDescription,
				consumer => {
					consumer.Channel.Send(consumer.PrepareRequestUserAuthorization(new Uri("http://printer.example.com/request_token_ready"), null, null)); // .Send() dropped because this is just a simulation
					string accessToken = consumer.ProcessUserAuthorization().AccessToken;
					var photoRequest = consumer.CreateAuthorizingMessage(accessPhotoEndpoint, accessToken);
					Response protectedPhoto = ((CoordinatingOAuthChannel)consumer.Channel).RequestProtectedResource(photoRequest);
					Assert.IsNotNull(protectedPhoto);
					Assert.AreEqual(HttpStatusCode.OK, protectedPhoto.Status);
					Assert.AreEqual("image/jpeg", protectedPhoto.Headers[HttpResponseHeader.ContentType]);
					Assert.AreNotEqual(0, protectedPhoto.ResponseStream.Length);
				},
				sp => {
					var requestTokenMessage = sp.ReadTokenRequest();
					sp.Channel.Send(sp.PrepareUnauthorizedTokenMessage(requestTokenMessage)); // .Send() dropped because this is just a simulation
					var authRequest = sp.ReadAuthorizationRequest();
					((InMemoryTokenManager)sp.TokenManager).AuthorizeRequestToken(authRequest.RequestToken);
					sp.Channel.Send(sp.PrepareAuthorizationResponse(authRequest)); // .Send() dropped because this is just a simulation
					var accessRequest = sp.ReadAccessTokenRequest();
					sp.Channel.Send(sp.PrepareAccessTokenMessage(accessRequest)); // .Send() dropped because this is just a simulation
					string accessToken = sp.ReadProtectedResourceAuthorization().AccessToken;
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
