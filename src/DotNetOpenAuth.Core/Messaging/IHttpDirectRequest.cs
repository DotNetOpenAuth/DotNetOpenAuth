//-----------------------------------------------------------------------
// <copyright file="IHttpDirectRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.Diagnostics.Contracts;
	using System.Net;

	/// <summary>
	/// An interface that allows direct request messages to capture the details of the HTTP request they arrived on.
	/// </summary>
	[ContractClass(typeof(IHttpDirectRequestContract))]
	public interface IHttpDirectRequest : IMessage {
		/// <summary>
		/// Gets the HTTP headers of the request.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		WebHeaderCollection Headers { get; }
	}
}
