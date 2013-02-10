namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Messaging;

	using Validation;

	public class OAuth1HttpMessageHandler : DelegatingHandler {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth1HttpMessageHandler" /> class.
		/// </summary>
		/// <param name="consumer">The consumer.</param>
		/// <param name="accessToken">The access token.</param>
		public OAuth1HttpMessageHandler(ConsumerBase consumer = null, string accessToken = null) {
			this.Consumer = consumer;
			this.AccessToken = accessToken;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth1HttpMessageHandler" /> class.
		/// </summary>
		/// <param name="innerHandler">The inner handler.</param>
		/// <param name="consumer">The consumer.</param>
		/// <param name="accessToken">The access token.</param>
		public OAuth1HttpMessageHandler(HttpMessageHandler innerHandler, ConsumerBase consumer = null, string accessToken = null)
			: base(innerHandler) {
			this.Consumer = consumer;
			this.AccessToken = accessToken;
		}

		public string AccessToken { get; set; }

		public ConsumerBase Consumer { get; set; }

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			Verify.Operation(this.Consumer != null, Strings.RequiredPropertyNotYetPreset, "Consumer");
			Verify.Operation(!string.IsNullOrEmpty(this.AccessToken), Strings.RequiredPropertyNotYetPreset, "AccessToken");

			var deliveryMethods = MessagingUtilities.GetHttpDeliveryMethod(request.Method.Method) | HttpDeliveryMethods.AuthorizationHeaderRequest;
			var signed = await
				this.Consumer.PrepareAuthorizedRequestAsync(
					new MessageReceivingEndpoint(request.RequestUri, deliveryMethods), this.AccessToken, cancellationToken);
			request.Headers.Authorization = signed.Headers.Authorization;

			return await base.SendAsync(request, cancellationToken);
		}
	}
}
