//-----------------------------------------------------------------------
// <copyright file="OpenIdCoordinator.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId;

	internal class OpenIdCoordinator : CoordinatorBase<OpenIdRelyingParty, OpenIdProvider> {
		internal OpenIdCoordinator(Action<OpenIdRelyingParty> rpAction, Action<OpenIdProvider> opAction)
			: base(rpAction, opAction) {
		}

		internal override void Run() {
			OpenIdRelyingParty rp = new OpenIdRelyingParty();
			OpenIdProvider op = new OpenIdProvider();

			RunCore(rp, op);
		}
	}
}
