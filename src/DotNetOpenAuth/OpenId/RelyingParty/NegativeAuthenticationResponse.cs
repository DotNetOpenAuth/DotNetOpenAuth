//-----------------------------------------------------------------------
// <copyright file="NegativeAuthenticationResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	internal class NegativeAuthenticationResponse : IAuthenticationResponse, ISetupRequiredAuthenticationResponse {
		private readonly NegativeAssertionResponse response;

		internal NegativeAuthenticationResponse(NegativeAssertionResponse response) {
			ErrorUtilities.VerifyArgumentNotNull(response, "response");
			this.response = response;
		}

		#region IAuthenticationResponse Members

		public Identifier ClaimedIdentifier {
			get { return null; }
		}

		public string FriendlyIdentifierForDisplay {
			get { return null; }
		}

		public AuthenticationStatus Status {
			get { return this.response.Immediate ? AuthenticationStatus.SetupRequired : AuthenticationStatus.Canceled; }
		}

		public Exception Exception {
			get { return null; }
		}

		public string GetCallbackArgument(string key) {
			return null;
		}

		public IDictionary<string, string> GetCallbackArguments() {
			return EmptyDictionary<string, string>.Instance;
		}

		public T GetExtension<T>() where T : IOpenIdMessageExtension, new() {
			return default(T);
		}

		public IOpenIdMessageExtension GetExtension(Type extensionType) {
			return null;
		}

		#endregion

		#region ISetupRequiredAuthenticationResponse Members

		public Identifier UserSuppliedIdentifier {
			get {
				if (this.Status != AuthenticationStatus.SetupRequired) {
					throw new InvalidOperationException(OpenIdStrings.OperationOnlyValidForSetupRequiredState);
				}

				string userSuppliedIdentifier;
				this.response.ExtraData.TryGetValue(AuthenticationRequest.UserSuppliedIdentifierParameterName, out userSuppliedIdentifier);
				return userSuppliedIdentifier;
			}
		}

		#endregion
	}
}
