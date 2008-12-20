//-----------------------------------------------------------------------
// <copyright file="IOutgoingExtensions.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal interface IOutgoingExtensions {
		/// <summary>
		/// Adds query parameters for OpenID extensions to the request directed 
		/// at the OpenID provider.
		/// </summary>
		void AddExtensionArguments(string extensionTypeUri, IDictionary<string, string> arguments);
	}
}
