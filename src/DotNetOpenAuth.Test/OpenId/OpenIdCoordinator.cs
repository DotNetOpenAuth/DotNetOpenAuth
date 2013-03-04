//-----------------------------------------------------------------------
// <copyright file="OpenIdCoordinator.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using Validation;

	internal class OpenIdCoordinator : CoordinatorBase<OpenIdRelyingParty, OpenIdProvider> {
		internal OpenIdCoordinator(Func<OpenIdRelyingParty, CancellationToken, Task> rpAction, Func<OpenIdProvider, CancellationToken, Task> opAction)
			: base(WrapAction(rpAction), WrapAction(opAction)) {
		}

		internal OpenIdProvider Provider { get; set; }

		internal OpenIdRelyingParty RelyingParty { get; set; }

		internal override Task RunAsync() {
			this.EnsurePartiesAreInitialized();
			var rpCoordinatingChannel = new CoordinatingChannel(this.RelyingParty.Channel, this.IncomingMessageFilter, this.OutgoingMessageFilter);
			var opCoordinatingChannel = new CoordinatingChannel(this.Provider.Channel, this.IncomingMessageFilter, this.OutgoingMessageFilter);
			rpCoordinatingChannel.RemoteChannel = opCoordinatingChannel;
			opCoordinatingChannel.RemoteChannel = rpCoordinatingChannel;

			this.RelyingParty.Channel = rpCoordinatingChannel;
			this.Provider.Channel = opCoordinatingChannel;

			return this.RunCoreAsync(this.RelyingParty, this.Provider);
		}

		private static Func<OpenIdRelyingParty, Task> WrapAction(Func<OpenIdRelyingParty, Task> action) {
			Requires.NotNull(action, "action");

			return async rp => {
				await action(rp);
				((CoordinatingChannel)rp.Channel).Close();
			};
		}

		private static Func<OpenIdProvider, Task> WrapAction(Func<OpenIdProvider, Task> action) {
			Requires.NotNull(action, "action");

			return async op => {
				await action(op);
				((CoordinatingChannel)op.Channel).Close();
			};
		}

		private void EnsurePartiesAreInitialized() {
			if (this.RelyingParty == null) {
				this.RelyingParty = new OpenIdRelyingParty(new StandardRelyingPartyApplicationStore());
				this.RelyingParty.DiscoveryServices.Add(new MockIdentifierDiscoveryService());
			}

			if (this.Provider == null) {
				this.Provider = new OpenIdProvider(new StandardProviderApplicationStore());
				this.Provider.DiscoveryServices.Add(new MockIdentifierDiscoveryService());
			}
		}
	}
}
