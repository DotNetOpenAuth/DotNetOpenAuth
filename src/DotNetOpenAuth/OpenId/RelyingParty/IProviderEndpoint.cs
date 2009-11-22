//-----------------------------------------------------------------------
// <copyright file="IProviderEndpoint.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Information published about an OpenId Provider by the
	/// OpenId discovery documents found at a user's Claimed Identifier.
	/// </summary>
	/// <remarks>
	/// Because information provided by this interface is suppplied by a 
	/// user's individually published documents, it may be incomplete or inaccurate.
	/// </remarks>
	[ContractClass(typeof(IProviderEndpointContract))]
	public interface IProviderEndpoint {
		/// <summary>
		/// Gets the detected version of OpenID implemented by the Provider.
		/// </summary>
		Version Version { get; }

		/// <summary>
		/// Gets the URL that the OpenID Provider receives authentication requests at.
		/// </summary>
		/// <value>
		/// This value MUST be an absolute HTTP or HTTPS URL.
		/// </value>
		Uri Uri { get; }

		/// <summary>
		/// Gets the collection of service type URIs found in the XRDS document describing this Provider.
		/// </summary>
		/// <value>Should never be null, but may be empty.</value>
		ReadOnlyCollection<string> Capabilities { get; }
	}

	/// <summary>
	/// Code contract for the <see cref="IProviderEndpoint"/> type.
	/// </summary>
	[ContractClassFor(typeof(IProviderEndpoint))]
	internal abstract class IProviderEndpointContract : IProviderEndpoint {
		/// <summary>
		/// Prevents a default instance of the <see cref="IProviderEndpointContract"/> class from being created.
		/// </summary>
		private IProviderEndpointContract() {
		}

		#region IProviderEndpoint Members

		/// <summary>
		/// Gets the detected version of OpenID implemented by the Provider.
		/// </summary>
		Version IProviderEndpoint.Version {
			get {
				Contract.Ensures(Contract.Result<Version>() != null);
				throw new System.NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the URL that the OpenID Provider receives authentication requests at.
		/// </summary>
		Uri IProviderEndpoint.Uri {
			get {
				Contract.Ensures(Contract.Result<Uri>() != null);
				throw new System.NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the collection of service type URIs found in the XRDS document describing this Provider.
		/// </summary>
		/// <value>Should never be null, but may be empty.</value>
		ReadOnlyCollection<string> IProviderEndpoint.Capabilities {
			get {
				Contract.Ensures(Contract.Result<ReadOnlyCollection<string>>() != null);
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}
