//-----------------------------------------------------------------------
// <copyright file="TwitterConsumer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Runtime.Serialization.Json;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.XPath;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;

	using Newtonsoft.Json.Linq;

	/// <summary>
	/// A consumer capable of communicating with Twitter.
	/// </summary>
	public class TwitterConsumer : Consumer {
		/// <summary>
		/// The description of Twitter's OAuth protocol URIs for use with actually reading/writing
		/// a user's private Twitter data.
		/// </summary>
		public static readonly ServiceProviderDescription ServiceDescription = new ServiceProviderDescription(
			"https://api.twitter.com/oauth/request_token",
			"https://api.twitter.com/oauth/authorize",
			"https://api.twitter.com/oauth/access_token");

		/// <summary>
		/// The description of Twitter's OAuth protocol URIs for use with their "Sign in with Twitter" feature.
		/// </summary>
		public static readonly ServiceProviderDescription SignInWithTwitterServiceDescription = new ServiceProviderDescription(
			"https://api.twitter.com/oauth/request_token",
			"https://api.twitter.com/oauth/authenticate",
			"https://api.twitter.com/oauth/access_token");

		/// <summary>
		/// The URI to get a user's favorites.
		/// </summary>
		private static readonly Uri GetFavoritesEndpoint = new Uri("http://twitter.com/favorites.xml");

		/// <summary>
		/// The URI to get the data on the user's home page.
		/// </summary>
		private static readonly Uri GetFriendTimelineStatusEndpoint = new Uri("https://api.twitter.com/1.1/statuses/home_timeline.json");

		private static readonly Uri UpdateProfileBackgroundImageEndpoint = new Uri("http://twitter.com/account/update_profile_background_image.xml");

		private static readonly Uri UpdateProfileImageEndpoint = new Uri("http://twitter.com/account/update_profile_image.xml");

		private static readonly Uri VerifyCredentialsEndpoint = new Uri("http://api.twitter.com/1/account/verify_credentials.xml");

		/// <summary>
		/// Initializes a new instance of the <see cref="TwitterConsumer"/> class.
		/// </summary>
		public TwitterConsumer() {
			this.ServiceProvider = ServiceDescription;
			this.ConsumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
			this.ConsumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"];
			this.TemporaryCredentialStorage = HttpContext.Current != null
												  ? (ITemporaryCredentialStorage)new CookieTemporaryCredentialStorage()
												  : new MemoryTemporaryCredentialStorage();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TwitterConsumer"/> class.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		public TwitterConsumer(string consumerKey, string consumerSecret)
			: this() {
			this.ConsumerKey = consumerKey;
			this.ConsumerSecret = consumerSecret;
		}

		/// <summary>
		/// Gets a value indicating whether the Twitter consumer key and secret are set in the web.config file.
		/// </summary>
		public static bool IsTwitterConsumerConfigured {
			get {
				return !string.IsNullOrEmpty(ConfigurationManager.AppSettings["twitterConsumerKey"]) &&
					!string.IsNullOrEmpty(ConfigurationManager.AppSettings["twitterConsumerSecret"]);
			}
		}

		public static Consumer CreateConsumer(bool forWeb = true) {
			string consumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
			string consumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"];
			if (IsTwitterConsumerConfigured) {
				ITemporaryCredentialStorage storage = forWeb ? (ITemporaryCredentialStorage)new CookieTemporaryCredentialStorage() : new MemoryTemporaryCredentialStorage();
				return new Consumer(consumerKey, consumerSecret, ServiceDescription, storage) {
					HostFactories = new TwitterHostFactories(),
				};
			} else {
				throw new InvalidOperationException("No Twitter OAuth consumer key and secret could be found in web.config AppSettings.");
			}
		}

		/// <summary>
		/// Prepares a redirect that will send the user to Twitter to sign in.
		/// </summary>
		/// <param name="forceNewLogin">if set to <c>true</c> the user will be required to re-enter their Twitter credentials even if already logged in to Twitter.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The redirect message.
		/// </returns>
		public static async Task<Uri> StartSignInWithTwitterAsync(bool forceNewLogin = false, CancellationToken cancellationToken = default(CancellationToken)) {
			var redirectParameters = new Dictionary<string, string>();
			if (forceNewLogin) {
				redirectParameters["force_login"] = "true";
			}
			Uri callback = MessagingUtilities.GetRequestUrlFromContext().StripQueryArgumentsWithPrefix("oauth_");

			var consumer = CreateConsumer();
			consumer.ServiceProvider = SignInWithTwitterServiceDescription;
			Uri redirectUrl = await consumer.RequestUserAuthorizationAsync(callback, cancellationToken: cancellationToken);
			return redirectUrl;
		}

		/// <summary>
		/// Checks the incoming web request to see if it carries a Twitter authentication response,
		/// and provides the user's Twitter screen name and unique id if available.
		/// </summary>
		/// <param name="completeUrl">The URL that came back from the service provider to complete the authorization.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A tuple with the screen name and Twitter unique user ID if successful; otherwise <c>null</c>.
		/// </returns>
		public static async Task<Tuple<string, int>> TryFinishSignInWithTwitterAsync(Uri completeUrl = null, CancellationToken cancellationToken = default(CancellationToken)) {
			var consumer = CreateConsumer();
			consumer.ServiceProvider = SignInWithTwitterServiceDescription;
			var response = await consumer.ProcessUserAuthorizationAsync(completeUrl ?? HttpContext.Current.Request.Url, cancellationToken: cancellationToken);
			if (response == null) {
				return null;
			}

			string screenName = response.ExtraData["screen_name"];
			int userId = int.Parse(response.ExtraData["user_id"]);
			return Tuple.Create(screenName, userId);
		}

		public async Task<JArray> GetUpdatesAsync(AccessToken accessToken, CancellationToken cancellationToken = default(CancellationToken)) {
			if (string.IsNullOrEmpty(accessToken.Token)) {
				throw new ArgumentNullException("accessToken.Token");
			}

			using (var httpClient = this.CreateHttpClient(accessToken)) {
				using (var response = await httpClient.GetAsync(GetFriendTimelineStatusEndpoint, cancellationToken)) {
					response.EnsureSuccessStatusCode();
					string jsonString = await response.Content.ReadAsStringAsync();
					var json = JArray.Parse(jsonString);
					return json;
				}
			}
		}

		public async Task<XDocument> GetFavorites(AccessToken accessToken, CancellationToken cancellationToken = default(CancellationToken)) {
			if (string.IsNullOrEmpty(accessToken.Token)) {
				throw new ArgumentNullException("accessToken.Token");
			}

			using (var httpClient = this.CreateHttpClient(accessToken)) {
				using (HttpResponseMessage response = await httpClient.GetAsync(GetFavoritesEndpoint, cancellationToken)) {
					response.EnsureSuccessStatusCode();
					return XDocument.Parse(await response.Content.ReadAsStringAsync());
				}
			}
		}

		public async Task<XDocument> UpdateProfileBackgroundImageAsync(AccessToken accessToken, string image, bool tile, CancellationToken cancellationToken) {
			var imageAttachment = new StreamContent(File.OpenRead(image));
			imageAttachment.Headers.ContentType = new MediaTypeHeaderValue("image/" + Path.GetExtension(image).Substring(1).ToLowerInvariant());
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, UpdateProfileBackgroundImageEndpoint);
			var content = new MultipartFormDataContent();
			content.Add(imageAttachment, "image");
			content.Add(new StringContent(tile.ToString().ToLowerInvariant()), "tile");
			request.Content = content;
			request.Headers.ExpectContinue = false;
			using (var httpClient = this.CreateHttpClient(accessToken)) {
				using (HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken)) {
					response.EnsureSuccessStatusCode();
					string responseString = await response.Content.ReadAsStringAsync();
					return XDocument.Parse(responseString);
				}
			}
		}

		public Task<XDocument> UpdateProfileImageAsync(AccessToken accessToken, string pathToImage, CancellationToken cancellationToken = default(CancellationToken)) {
			string contentType = "image/" + Path.GetExtension(pathToImage).Substring(1).ToLowerInvariant();
			return this.UpdateProfileImageAsync(accessToken, File.OpenRead(pathToImage), contentType, cancellationToken);
		}

		public async Task<XDocument> UpdateProfileImageAsync(AccessToken accessToken, Stream image, string contentType, CancellationToken cancellationToken = default(CancellationToken)) {
			var imageAttachment = new StreamContent(image);
			imageAttachment.Headers.ContentType = new MediaTypeHeaderValue(contentType);
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, UpdateProfileImageEndpoint);
			var content = new MultipartFormDataContent();
			content.Add(imageAttachment, "image", "twitterPhoto");
			request.Content = content;
			using (var httpClient = this.CreateHttpClient(accessToken)) {
				using (HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken)) {
					response.EnsureSuccessStatusCode();
					string responseString = await response.Content.ReadAsStringAsync();
					return XDocument.Parse(responseString);
				}
			}
		}

		public async Task<XDocument> VerifyCredentialsAsync(AccessToken accessToken, CancellationToken cancellationToken = default(CancellationToken)) {
			using (var httpClient = this.CreateHttpClient(accessToken)) {
				using (var response = await httpClient.GetAsync(VerifyCredentialsEndpoint, cancellationToken)) {
					response.EnsureSuccessStatusCode();
					using (var stream = await response.Content.ReadAsStreamAsync()) {
						return XDocument.Load(XmlReader.Create(stream));
					}
				}
			}
		}

		public async Task<string> GetUsername(AccessToken accessToken, CancellationToken cancellationToken = default(CancellationToken)) {
			XDocument xml = await this.VerifyCredentialsAsync(accessToken, cancellationToken);
			XPathNavigator nav = xml.CreateNavigator();
			return nav.SelectSingleNode("/user/screen_name").Value;
		}

		private class TwitterHostFactories : IHostFactories {
			private static readonly IHostFactories underlyingFactories = new DefaultOAuthHostFactories();

			public HttpMessageHandler CreateHttpMessageHandler() {
				return new WebRequestHandler();
			}

			public HttpClient CreateHttpClient(HttpMessageHandler handler = null) {
				var client = underlyingFactories.CreateHttpClient(handler);

				// Twitter can't handle the Expect 100 Continue HTTP header. 
				client.DefaultRequestHeaders.ExpectContinue = false;
				return client;
			}
		}
	}
}
