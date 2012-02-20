//-----------------------------------------------------------------------
// <copyright file="OAuthCoordinator.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.Test.Mocks;

	/// <summary>
	/// Runs a Consumer and Service Provider simultaneously so they can interact in a full simulation.
	/// </summary>
	internal class OAuthCoordinator : CoordinatorBase<WebConsumer, ServiceProvider> {
		private ConsumerDescription consumerDescription;
		private ServiceProviderDescription serviceDescription;
		private DotNetOpenAuth.OAuth.ServiceProviderSecuritySettings serviceProviderSecuritySettings = DotNetOpenAuth.Configuration.OAuthElement.Configuration.ServiceProvider.SecuritySettings.CreateSecuritySettings();
		private DotNetOpenAuth.OAuth.ConsumerSecuritySettings consumerSecuritySettings = DotNetOpenAuth.Configuration.OAuthElement.Configuration.Consumer.SecuritySettings.CreateSecuritySettings();

		/// <summary>Initializes a new instance of the <see cref="OAuthCoordinator"/> class.</summary>
		/// <param name="consumerDescription">The description of the consumer.</param>
		/// <param name="serviceDescription">The service description that will be used to construct the Consumer and ServiceProvider objects.</param>
		/// <param name="consumerAction">The code path of the Consumer.</param>
		/// <param name="serviceProviderAction">The code path of the Service Provider.</param>
		internal OAuthCoordinator(ConsumerDescription consumerDescription, ServiceProviderDescription serviceDescription, Action<WebConsumer> consumerAction, Action<ServiceProvider> serviceProviderAction)
			: base(consumerAction, serviceProviderAction) {
			Requires.NotNull(consumerDescription, "consumerDescription");
			Requires.NotNull(serviceDescription, "serviceDescription");

			this.consumerDescription = consumerDescription;
			this.serviceDescription = serviceDescription;
		}

		/// <summary>
		/// Starts the simulation.
		/// </summary>
		internal override void Run() {
			// Clone the template signing binding element.
			var signingElement = this.serviceDescription.CreateTamperProtectionElement();
			var consumerSigningElement = signingElement.Clone();
			var spSigningElement = signingElement.Clone();

			// Prepare token managers
			InMemoryTokenManager consumerTokenManager = new InMemoryTokenManager();
			InMemoryTokenManager serviceTokenManager = new InMemoryTokenManager();
			consumerTokenManager.AddConsumer(this.consumerDescription);
			serviceTokenManager.AddConsumer(this.consumerDescription);

			// Prepare channels that will pass messages directly back and forth.
			var consumerChannel = new CoordinatingOAuthConsumerChannel(consumerSigningElement, (IConsumerTokenManager)consumerTokenManager, this.consumerSecuritySettings);
			var serviceProviderChannel = new CoordinatingOAuthServiceProviderChannel(spSigningElement, (IServiceProviderTokenManager)serviceTokenManager, this.serviceProviderSecuritySettings);
			consumerChannel.RemoteChannel = serviceProviderChannel;
			serviceProviderChannel.RemoteChannel = consumerChannel;

			// Prepare the Consumer and Service Provider objects
			WebConsumer consumer = new WebConsumer(this.serviceDescription, consumerTokenManager) {
				OAuthChannel = consumerChannel,
			};
			ServiceProvider serviceProvider = new ServiceProvider(this.serviceDescription, serviceTokenManager, new NonceMemoryStore()) {
				OAuthChannel = serviceProviderChannel,
			};

			this.RunCore(consumer, serviceProvider);
		}
	}
}
