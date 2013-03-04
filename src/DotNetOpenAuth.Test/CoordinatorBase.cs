//-----------------------------------------------------------------------
// <copyright file="CoordinatorBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;
	using Validation;

	internal abstract class CoordinatorBase<T1, T2> {
		private Func<T1, CancellationToken, Task> party1Action;
		private Func<T2, CancellationToken, Task> party2Action;

		protected CoordinatorBase(Func<T1, CancellationToken, Task> party1Action, Func<T2, CancellationToken, Task> party2Action) {
			Requires.NotNull(party1Action, "party1Action");
			Requires.NotNull(party2Action, "party2Action");

			this.party1Action = party1Action;
			this.party2Action = party2Action;
		}

		protected internal Action<IProtocolMessage> IncomingMessageFilter { get; set; }

		protected internal Action<IProtocolMessage> OutgoingMessageFilter { get; set; }

		internal abstract Task RunAsync();

		protected async Task RunCoreAsync(T1 party1Object, T2 party2Object) {
			var cts = new CancellationTokenSource();

			try {
				var parties = new List<Task> {
					Task.Run(() => this.party1Action(party1Object, cts.Token)),
					Task.Run(() => this.party2Action(party2Object, cts.Token)),
				};
				var completingTask = await Task.WhenAny(parties);
				await completingTask; // rethrow any exception from the first completing task.

				// if no exception, then block for the second task now.
				await Task.WhenAll(parties);
			} catch {
				cts.Cancel(); // cause the second party to terminate, if necessary.
				throw;
			}
		}
	}
}
