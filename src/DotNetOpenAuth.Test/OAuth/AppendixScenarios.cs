//-----------------------------------------------------------------------
// <copyright file="AppendixScenarios.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth {
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class AppendixScenarios : TestBase {
		[Test]
		public async Task SpecAppendixAExample() {
			var serviceDescription = new ServiceProviderDescription(
				"https://photos.example.net/request_token",
				"http://photos.example.net/authorize",
				"https://photos.example.net/access_token");
			var serviceHostDescription = new ServiceProviderHostDescription {
				RequestTokenEndpoint = new MessageReceivingEndpoint(serviceDescription.TemporaryCredentialsRequestEndpoint, HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
				UserAuthorizationEndpoint = new MessageReceivingEndpoint(serviceDescription.ResourceOwnerAuthorizationEndpoint, HttpDeliveryMethods.GetRequest),
				AccessTokenEndpoint = new MessageReceivingEndpoint(serviceDescription.TokenRequestEndpoint, HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
				TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement(), },
			};
			var accessPhotoEndpoint = new Uri("http://photos.example.net/photos?file=vacation.jpg&size=original");
			var consumerDescription = new ConsumerDescription("dpf43f3p2l4k3l03", "kd94hf93k423kf44");

			var tokenManager = new InMemoryTokenManager();
			tokenManager.AddConsumer(consumerDescription);
			var sp = new ServiceProvider(serviceHostDescription, tokenManager);

			Handle(serviceDescription.TemporaryCredentialsRequestEndpoint).By(
				async (request, ct) => {
					var requestTokenMessage = await sp.ReadTokenRequestAsync(request, ct);
					return await sp.Channel.PrepareResponseAsync(sp.PrepareUnauthorizedTokenMessage(requestTokenMessage));
				});
			Handle(serviceDescription.ResourceOwnerAuthorizationEndpoint).By(
				async (request, ct) => {
					var authRequest = await sp.ReadAuthorizationRequestAsync(request, ct);
					((InMemoryTokenManager)sp.TokenManager).AuthorizeRequestToken(authRequest.RequestToken);
					return await sp.Channel.PrepareResponseAsync(sp.PrepareAuthorizationResponse(authRequest));
				});
			Handle(serviceDescription.TokenRequestEndpoint).By(
				async (request, ct) => {
					var accessRequest = await sp.ReadAccessTokenRequestAsync(request, ct);
					return await sp.Channel.PrepareResponseAsync(sp.PrepareAccessTokenMessage(accessRequest), ct);
				});
			Handle(accessPhotoEndpoint).By(
				async (request, ct) => {
					string accessToken = (await sp.ReadProtectedResourceAuthorizationAsync(request)).AccessToken;
					Assert.That(accessToken, Is.Not.Null.And.Not.Empty);
					var responseMessage = new HttpResponseMessage { Content = new ByteArrayContent(new byte[] { 0x33, 0x66 }), };
					responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
					return responseMessage;
				});

			var consumer = new Consumer(
				consumerDescription.ConsumerKey,
				consumerDescription.ConsumerSecret,
				serviceDescription,
				new MemoryTemporaryCredentialStorage());
			consumer.HostFactories = this.HostFactories;
			var authorizeUrl = await consumer.RequestUserAuthorizationAsync(new Uri("http://printer.example.com/request_token_ready"));
			Uri authorizeResponseUri;
			this.HostFactories.AllowAutoRedirects = false;
			using (var httpClient = this.HostFactories.CreateHttpClient()) {
				using (var response = await httpClient.GetAsync(authorizeUrl)) {
					Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
					authorizeResponseUri = response.Headers.Location;
				}
			}

			var accessTokenResponse = await consumer.ProcessUserAuthorizationAsync(authorizeResponseUri);
			Assert.That(accessTokenResponse, Is.Not.Null);

			using (var authorizingClient = consumer.CreateHttpClient(accessTokenResponse.AccessToken)) {
				using (var protectedPhoto = await authorizingClient.GetAsync(accessPhotoEndpoint)) {
					Assert.That(protectedPhoto, Is.Not.Null);
					protectedPhoto.EnsureSuccessStatusCode();
					Assert.That("image/jpeg", Is.EqualTo(protectedPhoto.Content.Headers.ContentType.MediaType));
					Assert.That(protectedPhoto.Content.Headers.ContentLength, Is.Not.EqualTo(0));
				}
			}
		}
	}
}
