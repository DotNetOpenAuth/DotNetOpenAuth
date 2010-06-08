//-----------------------------------------------------------------------
// <copyright file="IOAuthDirectResponseFormat.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A message the includes a request for the format the response message should come in.
	/// </summary>
	internal interface IOAuthDirectResponseFormat {
		/// <summary>
		/// Gets the format the client is requesting the authorization server should deliver the request in.
		/// </summary>
		/// <value>The format.</value>
		ResponseFormat Format { get; }
	}
}
