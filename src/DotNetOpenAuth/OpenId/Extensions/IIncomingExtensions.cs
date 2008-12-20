//-----------------------------------------------------------------------
// <copyright file="IIncomingExtensions.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal interface IIncomingExtensions {
		/// <summary>
		/// Gets the key/value pairs of a provider's response for a given OpenID extension.
		/// </summary>
		/// <param name="extensionTypeUri">
		/// The Type URI of the OpenID extension whose arguments are being sought.
		/// </param>
		/// <returns>
		/// Returns key/value pairs for this extension.
		/// </returns>
		IDictionary<string, string> GetExtensionArguments(string extensionTypeUri);

		/// <summary>
		/// Gets whether any arguments for a given extension are present.
		/// </summary>
		bool ContainsExtension(string extensionTypeUri);
	}
}
