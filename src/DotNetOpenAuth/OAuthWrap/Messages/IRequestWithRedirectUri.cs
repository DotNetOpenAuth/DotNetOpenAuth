//-----------------------------------------------------------------------
// <copyright file="IRequestWithRedirectUri.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A message that contains a callback parameter.
	/// </summary>
	internal interface IRequestWithRedirectUri {
		/// <summary>
		/// Gets the client identifier.
		/// </summary>
		/// <value>The client identifier.</value>
		string ClientIdentifier { get; }

		/// <summary>
		/// Gets the callback.
		/// </summary>
		/// <value>The callback.</value>
		Uri Callback { get; }
	}
}
