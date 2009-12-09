//-----------------------------------------------------------------------
// <copyright file="OAuthServiceProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	public class OAuthServiceProvider {
		private const string PendingAuthorizationRequestSessionKey = "PendingAuthorizationRequest";

		/// <summary>
		/// The shared service description for this web site.
		/// </summary>
		private static ServiceProviderDescription serviceDescription;

		private static OAuthServiceProviderTokenManager tokenManager;

		/// <summary>
		/// The shared service provider object.
		/// </summary>
		private static ServiceProvider serviceProvider;

		/// <summary>
		/// The lock to synchronize initialization of the <see cref="serviceProvider"/> field.
		/// </summary>
		private static object initializerLock = new object();

		/// <summary>
		/// Gets the service provider.
		/// </summary>
		/// <value>The service provider.</value>
		public static ServiceProvider ServiceProvider {
			get {
				EnsureInitialized();
				return serviceProvider;
			}
		}

		/// <summary>
		/// Gets the service description.
		/// </summary>
		/// <value>The service description.</value>
		public static ServiceProviderDescription ServiceDescription {
			get {
				EnsureInitialized();
				return serviceDescription;
			}
		}

		public static UserAuthorizationRequest PendingAuthorizationRequest {
			get { return HttpContext.Current.Session[PendingAuthorizationRequestSessionKey] as UserAuthorizationRequest; }
			set { HttpContext.Current.Session[PendingAuthorizationRequestSessionKey] = value; }
		}

		public static Consumer PendingAuthorizationConsumer {
			get {
				ITokenContainingMessage message = PendingAuthorizationRequest;
				if (message == null) {
					throw new InvalidOperationException();
				}

				return Database.DataContext.IssuedTokens.OfType<IssuedRequestToken>().Include("Consumer").First(t => t.Token == message.Token).Consumer;
			}
		}

		public static void AuthorizePendingRequestToken() {
			var pendingRequest = PendingAuthorizationRequest;
			if (pendingRequest == null) {
				throw new InvalidOperationException("No pending authorization request to authorize.");
			}

			ITokenContainingMessage msg = pendingRequest;
			var token = Database.DataContext.IssuedTokens.OfType<IssuedRequestToken>().First(t => t.Token == msg.Token);
			token.Authorize();

			PendingAuthorizationRequest = null;
			var response = serviceProvider.PrepareAuthorizationResponse(pendingRequest);
			if (response != null) {
				serviceProvider.Channel.Send(response);
			}
		}

		/// <summary>
		/// Initializes the <see cref="serviceProvider"/> field if it has not yet been initialized.
		/// </summary>
		private static void EnsureInitialized() {
			if (serviceProvider == null) {
				lock (initializerLock) {
					if (serviceDescription == null) {
						var postEndpoint = new MessageReceivingEndpoint(new Uri(Utilities.ApplicationRoot, "OAuth.ashx"), HttpDeliveryMethods.PostRequest);
						var getEndpoint = new MessageReceivingEndpoint(postEndpoint.Location, HttpDeliveryMethods.GetRequest);
						serviceDescription = new ServiceProviderDescription {
							TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
							RequestTokenEndpoint = postEndpoint,
							AccessTokenEndpoint = postEndpoint,
							UserAuthorizationEndpoint = getEndpoint,
						};
					}

					if (tokenManager == null) {
						tokenManager = new OAuthServiceProviderTokenManager();
					}

					if (serviceProvider == null) {
						serviceProvider = new ServiceProvider(serviceDescription, tokenManager);
					}
				}
			}
		}
	}
}
