//-----------------------------------------------------------------------
// <copyright file="Coordinator.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Scenarios {
	using System;
	using System.Threading;
	using DotNetOAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Runs a Consumer and Service Provider simultaneously so they can interact in a full simulation.
	/// </summary>
	internal class Coordinator {
		private Actor consumerAction;
		private Actor serviceProviderAction;

		/// <summary>Initializes a new instance of the <see cref="Coordinator"/> class.</summary>
		/// <param name="consumerAction">The code path of the Consumer.</param>
		/// <param name="serviceProviderAction">The code path of the Service Provider.</param>
		internal Coordinator(Actor consumerAction, Actor serviceProviderAction) {
			if (consumerAction == null) {
				throw new ArgumentNullException("consumerAction");
			}
			if (serviceProviderAction == null) {
				throw new ArgumentNullException("serviceProviderAction");
			}

			this.consumerAction = consumerAction;
			this.serviceProviderAction = serviceProviderAction;
		}

		internal delegate void Actor(CoordinatingOAuthChannel channel);

		/// <summary>
		/// Gets or sets the signing element the Consumer channel should use.
		/// </summary>
		/// <remarks>
		/// The Service Provider never signs a message, so no property is necessary for that.
		/// </remarks>
		internal ITamperProtectionChannelBindingElement SigningElement { get; set; }

		/// <summary>
		/// Starts the simulation.
		/// </summary>
		internal void Run() {
			if (this.SigningElement == null) {
				throw new InvalidOperationException("SigningElement must be set first.");
			}

			// Prepare channels that will pass messages directly back and forth.
			CoordinatingOAuthChannel consumerChannel = new CoordinatingOAuthChannel(this.SigningElement);
			CoordinatingOAuthChannel serviceProviderChannel = new CoordinatingOAuthChannel(this.SigningElement);
			consumerChannel.RemoteChannel = serviceProviderChannel;
			serviceProviderChannel.RemoteChannel = consumerChannel;

			Thread consumerThread = null, serviceProviderThread = null;
			Exception failingException = null;

			// Each thread we create needs a surrounding exception catcher so that we can
			// terminate the other thread and inform the test host that the test failed.
			Action<Actor, CoordinatingOAuthChannel> safeWrapper = (actor, channel) => {
				try {
					actor(channel);
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
			consumerThread = new Thread(() => { safeWrapper(consumerAction, consumerChannel); });
			serviceProviderThread = new Thread(() => { safeWrapper(serviceProviderAction, serviceProviderChannel); });
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
