//-----------------------------------------------------------------------
// <copyright file="IIdentifierDiscoveryResult.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.DiscoveryServices {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// An self-describing result of discovery on some identifier; perhaps one of many.
	/// </summary>
	[ContractClass(typeof(IIdentifierDiscoveryResultContract))]
	internal interface IIdentifierDiscoveryResult {
		/// <summary>
		/// Gets the provider endpoint.
		/// </summary>
		/// <value>The discovered provider endpoint.  May optionally implement <see cref="IXrdsProviderEndpoint"/>.</value>
		IProviderEndpoint ProviderEndpoint { get; }

		/// <summary>
		/// Gets the Identifier that the end user claims to control.
		/// </summary>
		Identifier ClaimedIdentifier { get; }

		/// <summary>
		/// Gets an alternate Identifier for an end user that is local to a 
		/// particular OP and thus not necessarily under the end user's 
		/// control.
		/// </summary>
		Identifier ProviderLocalIdentifier { get; }

		/// <summary>
		/// Gets the Identifier that was presented by the end user to the Relying Party, 
		/// or selected by the user at the OpenID Provider. 
		/// During the initiation phase of the protocol, an end user may enter 
		/// either their own Identifier or an OP Identifier. If an OP Identifier 
		/// is used, the OP may then assist the end user in selecting an Identifier 
		/// to share with the Relying Party.
		/// </summary>
		Identifier UserSuppliedIdentifier { get; }
	}

	/// <summary>
	/// Code contract class for the <see cref="IIdentifierDiscoveryResult"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IIdentifierDiscoveryResult))]
	internal class IIdentifierDiscoveryResultContract : IIdentifierDiscoveryResult {
		#region IIdentifierDiscoveryResult Members

		/// <summary>
		/// Gets the provider endpoint.
		/// </summary>
		/// <value>
		/// The discovered provider endpoint.  May optionally implement <see cref="IXrdsProviderEndpoint"/>.
		/// </value>
		IProviderEndpoint IIdentifierDiscoveryResult.ProviderEndpoint {
			get {
				Contract.Ensures(Contract.Result<IProviderEndpoint>() != null);
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the Identifier that the end user claims to control.
		/// </summary>
		Identifier IIdentifierDiscoveryResult.ClaimedIdentifier {
			get {
				Contract.Ensures(Contract.Result<Identifier>() != null);
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the Identifier that was presented by the end user to the Relying Party,
		/// or selected by the user at the OpenID Provider.
		/// During the initiation phase of the protocol, an end user may enter
		/// either their own Identifier or an OP Identifier. If an OP Identifier
		/// is used, the OP may then assist the end user in selecting an Identifier
		/// to share with the Relying Party.
		/// </summary>
		Identifier IIdentifierDiscoveryResult.UserSuppliedIdentifier {
			get {
				Contract.Ensures(Contract.Result<Identifier>() != null);
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets an alternate Identifier for an end user that is local to a
		/// particular OP and thus not necessarily under the end user's
		/// control.
		/// </summary>
		Identifier IIdentifierDiscoveryResult.ProviderLocalIdentifier {
			get {
				Contract.Ensures(Contract.Result<Identifier>() != null);
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}
