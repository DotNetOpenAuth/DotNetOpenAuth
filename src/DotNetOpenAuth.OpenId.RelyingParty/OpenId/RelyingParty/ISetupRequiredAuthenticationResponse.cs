//-----------------------------------------------------------------------
// <copyright file="ISetupRequiredAuthenticationResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// An interface to expose useful properties and functionality for handling
	/// authentication responses that are returned from Immediate authentication
	/// requests that require a subsequent request to be made in non-immediate mode.
	/// </summary>
	[ContractClass(typeof(ISetupRequiredAuthenticationResponseContract))]
	public interface ISetupRequiredAuthenticationResponse {
		/// <summary>
		/// Gets the <see cref="Identifier"/> to pass to <see cref="OpenIdRelyingParty.CreateRequest(Identifier)"/>
		/// in a subsequent authentication attempt.
		/// </summary>
		Identifier UserSuppliedIdentifier { get; }
	}

	/// <summary>
	/// Code contract class for the <see cref="ISetupRequiredAuthenticationResponse"/> type.
	/// </summary>
	[ContractClassFor(typeof(ISetupRequiredAuthenticationResponse))]
	internal abstract class ISetupRequiredAuthenticationResponseContract : ISetupRequiredAuthenticationResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="ISetupRequiredAuthenticationResponseContract"/> class.
		/// </summary>
		protected ISetupRequiredAuthenticationResponseContract() {
		}

		#region ISetupRequiredAuthenticationResponse Members

		/// <summary>
		/// Gets the <see cref="Identifier"/> to pass to <see cref="OpenIdRelyingParty.CreateRequest(Identifier)"/>
		/// in a subsequent authentication attempt.
		/// </summary>
		Identifier ISetupRequiredAuthenticationResponse.UserSuppliedIdentifier {
			get {
				Requires.ValidState(((IAuthenticationResponse)this).Status == AuthenticationStatus.SetupRequired, OpenIdStrings.OperationOnlyValidForSetupRequiredState);
				throw new System.NotImplementedException();
			}
		}

		#endregion
	}
}
