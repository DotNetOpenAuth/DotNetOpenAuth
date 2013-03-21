//-----------------------------------------------------------------------
// <copyright file="CoordinatorBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using DotNetOpenAuth.Test.OpenId;

	using NUnit.Framework;
	using Validation;

	using System.Linq;

	internal class CoordinatorBase {
		private Func<IHostFactories, CancellationToken, Task> driver;

		internal CoordinatorBase(Func<IHostFactories, CancellationToken, Task> driver, params TestBase.Handler[] handlers) {
			Requires.NotNull(driver, "driver");
			Requires.NotNull(handlers, "handlers");

			this.driver = driver;
			this.HostFactories = new MockingHostFactories(handlers.ToList());
		}

		internal MockingHostFactories HostFactories { get; set; }

		protected internal virtual async Task RunAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			await this.driver(this.HostFactories, cancellationToken);
		}
	}
}
