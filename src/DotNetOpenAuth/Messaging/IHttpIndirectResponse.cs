//-----------------------------------------------------------------------
// <copyright file="IHttpDirectResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.Diagnostics.Contracts;
	using System.Net;

	/// <summary>
	/// An interface that allows indirect response messages to specify
	/// HTTP transport specific properties.
	/// </summary>
	public interface IHttpIndirectResponse {
		bool Include301RedirectPayloadInFragment { get; }
	}
}
