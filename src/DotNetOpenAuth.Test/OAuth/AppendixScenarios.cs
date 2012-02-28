//-----------------------------------------------------------------------
// <copyright file="AppendixScenarios.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth {
	using System;
	using System.IO;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class AppendixScenarios : TestBase {
		[Test]
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

			OAuthCoordinator coordinator = new OAuthCoordinator(
				consumerDescription,
				serviceDescription,
				consumer => {
					consumer.Channel.PrepareResponse(consumer.PrepareRequestUserAuthorization(new Uri("http://printer.example.com/request_token_ready"), null, null)); // .Send() dropped because this is just a simulation
					string accessToken = consumer.ProcessUserAuthorization().AccessToken;
					var photoRequest = consumer.CreateAuthorizingMessage(accessPhotoEndpoint, accessToken);
					OutgoingWebResponse protectedPhoto = ((CoordinatingOAuthConsumerChannel)consumer.Channel).RequestProtectedResource(photoRequest);
					Assert.IsNotNull(protectedPhoto);
					Assert.AreEqual(HttpStatusCode.OK, protectedPhoto.Status);
					Assert.AreEqual("image/jpeg", protectedPhoto.Headers[HttpResponseHeader.ContentType]);
					Assert.AreNotEqual(0, protectedPhoto.ResponseStream.Length);
				},
				sp => {
					var requestTokenMessage = sp.ReadTokenRequest();
					sp.Channel.PrepareResponse(sp.PrepareUnauthorizedTokenMessage(requestTokenMessage)); // .Send() dropped because this is just a simulation
					var authRequest = sp.ReadAuthorizationRequest();
					((InMemoryTokenManager)sp.TokenManager).AuthorizeRequestToken(authRequest.RequestToken);
					sp.Channel.PrepareResponse(sp.PrepareAuthorizationResponse(authRequest)); // .Send() dropped because this is just a simulation
					var accessRequest = sp.ReadAccessTokenRequest();
					sp.Channel.PrepareResponse(sp.PrepareAccessTokenMessage(accessRequest)); // .Send() dropped because this is just a simulation
					string accessToken = sp.ReadProtectedResourceAuthorization().AccessToken;
					((CoordinatingOAuthServiceProviderChannel)sp.Channel).SendDirectRawResponse(new OutgoingWebResponse {
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
