//-----------------------------------------------------------------------
// <copyright file="IXrdsProviderEndpoint.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// An <see cref="IProviderEndpoint"/> interface with additional members for use
	/// in sorting for most preferred endpoint.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xrds", Justification = "Xrds is an acronym.")]
	public interface IXrdsProviderEndpoint : IProviderEndpoint {
		/// <summary>
		/// Gets the priority associated with this service that may have been given
		/// in the XRDS document.
		/// </summary>
		int? ServicePriority { get; }

		/// <summary>
		/// Gets the priority associated with the service endpoint URL.
		/// </summary>
		/// <remarks>
		/// When sorting by priority, this property should be considered second after 
		/// <see cref="ServicePriority"/>.
		/// </remarks>
		int? UriPriority { get; }

		/// <summary>
		/// Checks for the presence of a given Type URI in an XRDS service.
		/// </summary>
		/// <param name="typeUri">The type URI to check for.</param>
		/// <returns><c>true</c> if the service type uri is present; <c>false</c> otherwise.</returns>
		bool IsTypeUriPresent(string typeUri);
	}
}
