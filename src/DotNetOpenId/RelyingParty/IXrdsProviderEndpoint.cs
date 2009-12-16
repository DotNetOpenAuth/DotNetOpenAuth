using System.Diagnostics.CodeAnalysis;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// An <see cref="IProviderEndpoint"/> interface with additional members for use
	/// in sorting for most preferred endpoint.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xrds")]
	public interface IXrdsProviderEndpoint : IProviderEndpoint {
		/// <summary>
		/// Checks for the presence of a given Type URI in an XRDS service.
		/// </summary>
		bool IsTypeUriPresent(string typeUri);
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
	}
}
