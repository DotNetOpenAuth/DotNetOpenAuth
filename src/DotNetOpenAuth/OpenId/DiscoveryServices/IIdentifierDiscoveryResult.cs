//-----------------------------------------------------------------------
// <copyright file="IIdentifierDiscoveryResult.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.DiscoveryServices {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using System.Diagnostics.Contracts;

	[ContractClass(typeof(IIdentifierDiscoveryResultContract))]
	interface IIdentifierDiscoveryResult {
		/// <summary>
		/// Gets the provider endpoint.
		/// </summary>
		/// <value>The discovered provider endpoint.  May optionally implement <see cref="IXrdsProviderEndpoint"/>.</value>
		IProviderEndpoint ProviderEndpoint { get; }

		Identifier ClaimedIdentifier { get; }

		Identifier ProviderLocalIdentifier { get; }

		Identifier UserSuppliedIdentifier { get; }
	}

	[ContractClassFor(typeof(IIdentifierDiscoveryResult))]
	internal class IIdentifierDiscoveryResultContract : IIdentifierDiscoveryResult {
		#region IIdentifierDiscoveryResult Members

		IProviderEndpoint IIdentifierDiscoveryResult.ProviderEndpoint {
			get {
				Contract.Ensures(Contract.Result<IProviderEndpoint>() != null);
				throw new NotImplementedException();
			}
		}

		Identifier IIdentifierDiscoveryResult.ClaimedIdentifier {
			get {
				Contract.Ensures(Contract.Result<Identifier>() != null);
				throw new NotImplementedException();
			}
		}

		Identifier IIdentifierDiscoveryResult.UserSuppliedIdentifier {
			get {
				Contract.Ensures(Contract.Result<Identifier>() != null);
				throw new NotImplementedException();
			}
		}

		Identifier IIdentifierDiscoveryResult.ProviderLocalIdentifier {
			get {
				Contract.Ensures(Contract.Result<Identifier>() != null);
				throw new NotImplementedException();
			}
		}

		#endregion
	}

}
