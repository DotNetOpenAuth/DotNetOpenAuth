//-----------------------------------------------------------------------
// <copyright file="IConsumerDescription.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// A description of a client from an Authorization Server's point of view.
	/// </summary>
	[ContractClass(typeof(IConsumerDescriptionContract))]
	public interface IConsumerDescription {
		/// <summary>
		/// Gets the client secret.
		/// </summary>
		string Secret { get; }

		/// <summary>
		/// Gets the allowed callback URIs that this client has pre-registered with the service provider, if any.
		/// </summary>
		/// <value>The URIs that user authorization responses may be directed to; must not be <c>null</c>, but may be empty.</value>
		/// <remarks>
		/// The first element in this list (if any) will be used as the default client redirect URL if the client sends an authorization request without a redirect URL.
		/// If the list is empty, any callback is allowed for this client.  
		/// </remarks>
		List<Uri> AllowedCallbacks { get; }
	}

	/// <summary>
	/// Contract class for the <see cref="IConsumerDescription"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IConsumerDescription))]
	internal abstract class IConsumerDescriptionContract : IConsumerDescription {
		#region IConsumerDescription Members

		/// <summary>
		/// Gets the client secret.
		/// </summary>
		/// <value></value>
		string IConsumerDescription.Secret {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets the allowed callback URIs that this client has pre-registered with the service provider, if any.
		/// </summary>
		/// <value>
		/// The URIs that user authorization responses may be directed to; must not be <c>null</c>, but may be empty.
		/// </value>
		/// <remarks>
		/// The first element in this list (if any) will be used as the default client redirect URL if the client sends an authorization request without a redirect URL.
		/// If the list is empty, any callback is allowed for this client.
		/// </remarks>
		List<Uri> IConsumerDescription.AllowedCallbacks {
			get {
				Contract.Ensures(Contract.Result<List<Uri>>() != null);
				Contract.Ensures(Contract.Result<List<Uri>>().TrueForAll(v => v != null && v.IsAbsoluteUri));
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}
