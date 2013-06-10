//-----------------------------------------------------------------------
// <copyright file="IHttpIndirectResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.Net;

	/// <summary>
	/// An interface that allows indirect response messages to specify
	/// HTTP transport specific properties.
	/// </summary>
	public interface IHttpIndirectResponse {
		/// <summary>
		/// Gets a value indicating whether the payload for the message should be included
		/// in the redirect fragment instead of the query string or POST entity.
		/// </summary>
		bool Include301RedirectPayloadInFragment { get; }
	}
}
