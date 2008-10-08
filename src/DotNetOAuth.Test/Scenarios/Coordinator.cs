//-----------------------------------------------------------------------
// <copyright file="Coordinator.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Scenarios {
	using System;
	using System.Linq;
	using System.Threading;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Runs a Consumer and Service Provider simultaneously so they can interact in a full simulation.
	/// </summary>
	internal class Coordinator {
		private ConsumerDescription consumerDescription;
		private ServiceProviderDescription serviceDescription;
		private Action<Consumer> consumerAction;
		private Action<ServiceProvider> serviceProviderAction;

		/// <summary>Initializes a new instance of the <see cref="Coordinator"/> class.</summary>
		/// <param name="consumerDescription">The description of the consumer.</param>
		/// <param name="serviceDescription">The service description that will be used to construct the Consumer and ServiceProvider objects.</param>
		/// <param name="consumerAction">The code path of the Consumer.</param>
		/// <param name="serviceProviderAction">The code path of the Service Provider.</param>
		internal Coordinator(ConsumerDescription consumerDescription, ServiceProviderDescription serviceDescription, Action<Consumer> consumerAction, Action<ServiceProvider> serviceProviderAction) {
			if (consumerDescription == null) {
				throw new ArgumentNullException("consumerDescription");
			}
			if (serviceDescription == null) {
				throw new ArgumentNullException("serviceDescription");
			}
			if (consumerAction == null) {
				throw new ArgumentNullException("consumerAction");
			}
			if (serviceProviderAction == null) {
				throw new ArgumentNullException("serviceProviderAction");
			}

			this.consumerDescription = consumerDescription;
			this.serviceDescription = serviceDescription;
			this.consumerAction = consumerAction;
			this.serviceProviderAction = serviceProviderAction;
		}

		/// <summary>
		/// Starts the simulation.
		/// </summary>
		internal void Run() {
			// Clone the template signing binding element.
			var signingElement = this.serviceDescription.CreateTamperProtectionElement();
			var consumerSigningElement = signingElement.Clone();
			var spSigningElement = signingElement.Clone();

			// Prepare token managers
			InMemoryTokenManager consumerTokenManager = new InMemoryTokenManager();
			InMemoryTokenManager serviceTokenManager = new InMemoryTokenManager();
			serviceTokenManager.AddConsumer(this.consumerDescription.ConsumerKey, this.consumerDescription.ConsumerSecret);

			// Prepare channels that will pass messages directly back and forth.
			CoordinatingOAuthChannel consumerChannel = new CoordinatingOAuthChannel(consumerSigningElement, true, consumerTokenManager);
			CoordinatingOAuthChannel serviceProviderChannel = new CoordinatingOAuthChannel(spSigningElement, false, serviceTokenManager);
			consumerChannel.RemoteChannel = serviceProviderChannel;
			serviceProviderChannel.RemoteChannel = consumerChannel;

			// Prepare the Consumer and Service Provider objects
			Consumer consumer = new Consumer(this.serviceDescription, consumerTokenManager) {
				Channel = consumerChannel,
				ConsumerKey = this.consumerDescription.ConsumerKey,
				ConsumerSecret = this.consumerDescription.ConsumerSecret,
			};
			ServiceProvider serviceProvider = new ServiceProvider(this.serviceDescription, serviceTokenManager) {
				Channel = serviceProviderChannel,
			};

			Thread consumerThread = null, serviceProviderThread = null;
			Exception failingException = null;

			// Each thread we create needs a surrounding exception catcher so that we can
			// terminate the other thread and inform the test host that the test failed.
			Action<Action> safeWrapper = (action) => {
				try {
					action();
				} catch (Exception ex) {
					// We may be the second thread in an ThreadAbortException, so check the "flag"
					if (failingException == null) {
						failingException = ex;
						if (Thread.CurrentThread == consumerThread) {
							serviceProviderThread.Abort();
						} else {
							consumerThread.Abort();
						}
					}
				}
			};

			// Run the threads, and wait for them to complete.
			// If this main thread is aborted (test run aborted), go ahead and abort the other two threads.
			consumerThread = new Thread(() => { safeWrapper(() => { consumerAction(consumer); }); });
			serviceProviderThread = new Thread(() => { safeWrapper(() => { serviceProviderAction(serviceProvider); }); });
			try {
				consumerThread.Start();
				serviceProviderThread.Start();
				consumerThread.Join();
				serviceProviderThread.Join();
			} catch (ThreadAbortException) {
				consumerThread.Abort();
				serviceProviderThread.Abort();
				throw;
			}

			// Use the failing reason of a failing sub-thread as our reason, if anything failed.
			if (failingException != null) {
				throw new AssertFailedException("Coordinator thread threw unhandled exception: " + failingException, failingException);
			}
		}
	}
}
