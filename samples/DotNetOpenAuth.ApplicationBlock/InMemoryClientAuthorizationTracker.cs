//-----------------------------------------------------------------------
// <copyright file="InMemoryClientAuthorizationTracker.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.ServiceModel;
	using System.Text;
	using System.Threading;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;

#if SAMPLESONLY
	internal class InMemoryClientAuthorizationTracker : IClientAuthorizationTracker {
		private readonly Dictionary<int, IAuthorizationState> savedStates = new Dictionary<int, IAuthorizationState>();
		private int stateCounter;

		#region Implementation of IClientTokenManager

		/// <summary>
		/// Gets the state of the authorization for a given callback URL and client state.
		/// </summary>
		/// <param name="callbackUrl">The callback URL.</param>
		/// <param name="clientState">State of the client stored at the beginning of an authorization request.</param>
		/// <returns>The authorization state; may be <c>null</c> if no authorization state matches.</returns>
		public IAuthorizationState GetAuthorizationState(Uri callbackUrl, string clientState) {
			IAuthorizationState state;
			if (this.savedStates.TryGetValue(int.Parse(clientState), out state)) {
				if (state.Callback != callbackUrl) {
					throw new DotNetOpenAuth.Messaging.ProtocolException("Client state and callback URL do not match.");
				}
			}

			return state;
		}

		#endregion

		internal IAuthorizationState NewAuthorization(HashSet<string> scope, out string clientState) {
			int counter = Interlocked.Increment(ref this.stateCounter);
			clientState = counter.ToString(CultureInfo.InvariantCulture);
			return this.savedStates[counter] = new AuthorizationState(scope);
		}
	}
#endif
}
