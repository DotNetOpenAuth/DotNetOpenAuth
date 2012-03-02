//-----------------------------------------------------------------------
// <copyright file="IEmbeddedResourceRetrieval.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;

	/// <summary>
	/// An interface that provides URLs from which embedded resources can be obtained.
	/// </summary>
	public interface IEmbeddedResourceRetrieval {
		/// <summary>
		/// Gets the URL from which the given manifest resource may be downloaded by the user agent.
		/// </summary>
		/// <param name="someTypeInResourceAssembly">Some type in the assembly containing the desired resource.</param>
		/// <param name="manifestResourceName">Manifest name of the desired resource.</param>
		/// <returns>An absolute URL.</returns>
		Uri GetWebResourceUrl(Type someTypeInResourceAssembly, string manifestResourceName);
	}
}
