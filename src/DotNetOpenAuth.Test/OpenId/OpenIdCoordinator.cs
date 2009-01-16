//-----------------------------------------------------------------------
// <copyright file="OpenIdCoordinator.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;

	internal class OpenIdCoordinator : CoordinatorBase<OpenIdRelyingParty, OpenIdProvider> {
		internal OpenIdCoordinator(Action<OpenIdRelyingParty> rpAction, Action<OpenIdProvider> opAction)
			: base(WrapAction(rpAction), WrapAction(opAction)) {
		}

		internal OpenIdProvider Provider { get; set; }

		internal OpenIdRelyingParty RelyingParty { get; set; }

		internal override void Run() {
			this.EnsurePartiesAreInitialized();
			var rpCoordinatingChannel = new CoordinatingChannel(this.RelyingParty.Channel, this.IncomingMessageFilter, this.OutgoingMessageFilter);
			var opCoordinatingChannel = new CoordinatingChannel(this.Provider.Channel, this.IncomingMessageFilter, this.OutgoingMessageFilter);
			rpCoordinatingChannel.RemoteChannel = opCoordinatingChannel;
			opCoordinatingChannel.RemoteChannel = rpCoordinatingChannel;

			this.RelyingParty.Channel = rpCoordinatingChannel;
			this.Provider.Channel = opCoordinatingChannel;

			RunCore(this.RelyingParty, this.Provider);
		}

		private static Action<OpenIdRelyingParty> WrapAction(Action<OpenIdRelyingParty> action) {
			ErrorUtilities.VerifyArgumentNotNull(action, "action");

			return rp => {
				action(rp);
				((CoordinatingChannel)rp.Channel).Close();
			};
		}

		private static Action<OpenIdProvider> WrapAction(Action<OpenIdProvider> action) {
			ErrorUtilities.VerifyArgumentNotNull(action, "action");

			return op => {
				action(op);
				((CoordinatingChannel)op.Channel).Close();
			};
		}

		private void EnsurePartiesAreInitialized() {
			if (this.RelyingParty == null) {
				this.RelyingParty = new OpenIdRelyingParty(new StandardRelyingPartyApplicationStore());
			}

			if (this.Provider == null) {
				this.Provider = new OpenIdProvider(new StandardProviderApplicationStore());
			}
		}
	}
}
