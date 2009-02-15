//-----------------------------------------------------------------------
// <copyright file="ServiceEndpoint.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Represents a single OP endpoint from discovery on some OpenID Identifier.
	/// </summary>
	[DebuggerDisplay("ClaimedIdentifier: {ClaimedIdentifier}, ProviderEndpoint: {ProviderEndpoint}, OpenId: {Protocol.Version}")]
	internal class ServiceEndpoint : IXrdsProviderEndpoint {
		/// <summary>
		/// The i-name identifier the user actually typed in
		/// or the url identifier with the scheme stripped off.
		/// </summary>
		private string friendlyIdentifierForDisplay;

		/// <summary>
		/// Backing field for the <see cref="ClaimedIdentifier"/> property.
		/// </summary>
		private Identifier claimedIdentifier;

		/// <summary>
		/// The OpenID protocol version used at the identity Provider.
		/// </summary>
		private Protocol protocol;

		/// <summary>
		/// The @priority given in the XRDS document for this specific OP endpoint.
		/// </summary>
		private int? uriPriority;

		/// <summary>
		/// The @priority given in the XRDS document for this service
		/// (which may consist of several endpoints).
		/// </summary>
		private int? servicePriority;

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceEndpoint"/> class.
		/// </summary>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="claimedIdentifier">The Claimed Identifier.</param>
		/// <param name="userSuppliedIdentifier">The User-supplied Identifier.</param>
		/// <param name="providerLocalIdentifier">The Provider Local Identifier.</param>
		/// <param name="servicePriority">The service priority.</param>
		/// <param name="uriPriority">The URI priority.</param>
		private ServiceEndpoint(ProviderEndpointDescription providerEndpoint, Identifier claimedIdentifier, Identifier userSuppliedIdentifier, Identifier providerLocalIdentifier, int? servicePriority, int? uriPriority) {
			ErrorUtilities.VerifyArgumentNotNull(claimedIdentifier, "claimedIdentifier");
			ErrorUtilities.VerifyArgumentNotNull(providerEndpoint, "providerEndpoint");
			this.ProviderDescription = providerEndpoint;
			this.ClaimedIdentifier = claimedIdentifier;
			this.UserSuppliedIdentifier = userSuppliedIdentifier;
			this.ProviderLocalIdentifier = providerLocalIdentifier ?? claimedIdentifier;
			this.servicePriority = servicePriority;
			this.uriPriority = uriPriority;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceEndpoint"/> class.
		/// </summary>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="claimedIdentifier">The Claimed Identifier.</param>
		/// <param name="userSuppliedIdentifier">The User-supplied Identifier.</param>
		/// <param name="providerLocalIdentifier">The Provider Local Identifier.</param>
		/// <param name="protocol">The protocol.</param>
		/// <remarks>
		/// Used for deserializing <see cref="ServiceEndpoint"/> from authentication responses.
		/// </remarks>
		private ServiceEndpoint(Uri providerEndpoint, Identifier claimedIdentifier, Identifier userSuppliedIdentifier, Identifier providerLocalIdentifier, Protocol protocol) {
			ErrorUtilities.VerifyArgumentNotNull(providerEndpoint, "providerEndpoint");
			ErrorUtilities.VerifyArgumentNotNull(claimedIdentifier, "claimedIdentifier");
			ErrorUtilities.VerifyArgumentNotNull(providerLocalIdentifier, "providerLocalIdentifier");
			ErrorUtilities.VerifyArgumentNotNull(protocol, "protocol");

			this.ClaimedIdentifier = claimedIdentifier;
			this.UserSuppliedIdentifier = userSuppliedIdentifier;
			this.ProviderDescription = new ProviderEndpointDescription(providerEndpoint, protocol.Version);
			this.ProviderLocalIdentifier = providerLocalIdentifier ?? claimedIdentifier;
			this.protocol = protocol;
		}

		/// <summary>
		/// Gets the URL that the OpenID Provider receives authentication requests at.
		/// </summary>
		Uri IProviderEndpoint.Uri { get { return this.ProviderEndpoint; } }

		/// <summary>
		/// Gets the URL which accepts OpenID Authentication protocol messages.
		/// </summary>
		/// <remarks>
		/// Obtained by performing discovery on the User-Supplied Identifier. 
		/// This value MUST be an absolute HTTP or HTTPS URL.
		/// </remarks>
		public Uri ProviderEndpoint {
			get { return this.ProviderDescription.Endpoint; }
		}

		/*
		/// <summary>
		/// An Identifier for an OpenID Provider.
		/// </summary>
		public Identifier ProviderIdentifier { get; private set; }
		*/

		/// <summary>
		/// Gets the Identifier that was presented by the end user to the Relying Party, 
		/// or selected by the user at the OpenID Provider. 
		/// During the initiation phase of the protocol, an end user may enter 
		/// either their own Identifier or an OP Identifier. If an OP Identifier 
		/// is used, the OP may then assist the end user in selecting an Identifier 
		/// to share with the Relying Party.
		/// </summary>
		public Identifier UserSuppliedIdentifier { get; private set; }

		/// <summary>
		/// Gets or sets the Identifier that the end user claims to own.
		/// </summary>
		public Identifier ClaimedIdentifier {
			get {
				return this.claimedIdentifier;
			}

			set {
				// Take care to reparse the incoming identifier to make sure it's
				// not a derived type that will override expected behavior.
				// Elsewhere in this class, we count on the fact that this property
				// is either UriIdentifier or XriIdentifier.  MockIdentifier messes it up.
				this.claimedIdentifier = value != null ? Identifier.Parse(value) : null;
			}
		}

		/// <summary>
		/// Gets an alternate Identifier for an end user that is local to a 
		/// particular OP and thus not necessarily under the end user's 
		/// control.
		/// </summary>
		public Identifier ProviderLocalIdentifier { get; private set; }

		/// <summary>
		/// Gets the value for the <see cref="IAuthenticationResponse.FriendlyIdentifierForDisplay"/> property.
		/// </summary>
		public string FriendlyIdentifierForDisplay {
			get {
				if (this.friendlyIdentifierForDisplay == null) {
					XriIdentifier xri = this.ClaimedIdentifier as XriIdentifier;
					UriIdentifier uri = this.ClaimedIdentifier as UriIdentifier;
					if (xri != null) {
						if (this.UserSuppliedIdentifier == null || String.Equals(this.UserSuppliedIdentifier, this.ClaimedIdentifier, StringComparison.OrdinalIgnoreCase)) {
							this.friendlyIdentifierForDisplay = this.ClaimedIdentifier;
						} else {
							this.friendlyIdentifierForDisplay = this.UserSuppliedIdentifier;
						}
					} else if (uri != null) {
						if (uri != this.Protocol.ClaimedIdentifierForOPIdentifier) {
							string displayUri = uri.Uri.Authority + uri.Uri.PathAndQuery;
							displayUri = displayUri.TrimEnd('/');

							// Multi-byte unicode characters get encoded by the Uri class for transit.
							// Since this is for display purposes, we want to reverse this and display a readable
							// representation of these foreign characters.  
							this.friendlyIdentifierForDisplay = Uri.UnescapeDataString(displayUri);
						}
					} else {
						ErrorUtilities.ThrowInternal("ServiceEndpoint.ClaimedIdentifier neither XRI nor URI.");
						this.friendlyIdentifierForDisplay = this.ClaimedIdentifier;
					}
				}
				return this.friendlyIdentifierForDisplay;
			}
		}

		/// <summary>
		/// Gets the list of services available at this OP Endpoint for the
		/// claimed Identifier.  May be null.
		/// </summary>
		public ReadOnlyCollection<string> ProviderSupportedServiceTypeUris {
			get { return this.ProviderDescription.Capabilities; }
		}

		/// <summary>
		/// Gets the OpenID protocol used by the Provider.
		/// </summary>
		public Protocol Protocol {
			get {
				if (this.protocol == null) {
					this.protocol = Protocol.Lookup(this.ProviderDescription.ProtocolVersion);
				}

				return this.protocol;
			}
		}

		#region IXrdsProviderEndpoint Members

		/// <summary>
		/// Gets the priority associated with this service that may have been given
		/// in the XRDS document.
		/// </summary>
		int? IXrdsProviderEndpoint.ServicePriority {
			get { return this.servicePriority; }
		}

		/// <summary>
		/// Gets the priority associated with the service endpoint URL.
		/// </summary>
		int? IXrdsProviderEndpoint.UriPriority {
			get { return this.uriPriority; }
		}

		#endregion

		/// <summary>
		/// Gets the detected version of OpenID implemented by the Provider.
		/// </summary>
		Version IProviderEndpoint.Version { get { return Protocol.Version; } }

		/// <summary>
		/// Gets an XRDS sorting routine that uses the XRDS Service/@Priority 
		/// attribute to determine order.
		/// </summary>
		/// <remarks>
		/// Endpoints lacking any priority value are sorted to the end of the list.
		/// </remarks>
		internal static Comparison<IXrdsProviderEndpoint> EndpointOrder {
			get {
				// Sort first by service type (OpenID 2.0, 1.1, 1.0),
				// then by Service/@priority, then by Service/Uri/@priority
				return (se1, se2) => {
					int result = GetEndpointPrecedenceOrderByServiceType(se1).CompareTo(GetEndpointPrecedenceOrderByServiceType(se2));
					if (result != 0) {
						return result;
					}
					if (se1.ServicePriority.HasValue && se2.ServicePriority.HasValue) {
						result = se1.ServicePriority.Value.CompareTo(se2.ServicePriority.Value);
						if (result != 0) {
							return result;
						}
						if (se1.UriPriority.HasValue && se2.UriPriority.HasValue) {
							return se1.UriPriority.Value.CompareTo(se2.UriPriority.Value);
						} else if (se1.UriPriority.HasValue) {
							return -1;
						} else if (se2.UriPriority.HasValue) {
							return 1;
						} else {
							return 0;
						}
					} else {
						if (se1.ServicePriority.HasValue) {
							return -1;
						} else if (se2.ServicePriority.HasValue) {
							return 1;
						} else {
							// neither service defines a priority, so base ordering by uri priority.
							if (se1.UriPriority.HasValue && se2.UriPriority.HasValue) {
								return se1.UriPriority.Value.CompareTo(se2.UriPriority.Value);
							} else if (se1.UriPriority.HasValue) {
								return -1;
							} else if (se2.UriPriority.HasValue) {
								return 1;
							} else {
								return 0;
							}
						}
					}
				};
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="ProviderEndpoint"/> is using an encrypted channel.
		/// </summary>
		internal bool IsSecure {
			get { return string.Equals(this.ProviderEndpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase); }
		}

		/// <summary>
		/// Gets the provider description.
		/// </summary>
		internal ProviderEndpointDescription ProviderDescription { get; private set; }

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="se1">The first service endpoint.</param>
		/// <param name="se2">The second service endpoint.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(ServiceEndpoint se1, ServiceEndpoint se2) {
			return se1.EqualsNullSafe(se2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="se1">The first service endpoint.</param>
		/// <param name="se2">The second service endpoint.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(ServiceEndpoint se1, ServiceEndpoint se2) {
			return !(se1 == se2);
		}

		/// <summary>
		/// Checks for the presence of a given Type URI in an XRDS service.
		/// </summary>
		/// <param name="typeUri">The type URI to check for.</param>
		/// <returns>
		/// 	<c>true</c> if the service type uri is present; <c>false</c> otherwise.
		/// </returns>
		public bool IsTypeUriPresent(string typeUri) {
			return this.IsExtensionSupported(typeUri);
		}

		/// <summary>
		/// Determines whether some extension is supported by the Provider.
		/// </summary>
		/// <param name="extensionUri">The extension URI.</param>
		/// <returns>
		/// 	<c>true</c> if the extension is supported; otherwise, <c>false</c>.
		/// </returns>
		public bool IsExtensionSupported(string extensionUri) {
			ErrorUtilities.VerifyNonZeroLength(extensionUri, "extensionUri");

			ErrorUtilities.VerifyOperation(this.ProviderSupportedServiceTypeUris != null, OpenIdStrings.ExtensionLookupSupportUnavailable);
			return this.ProviderSupportedServiceTypeUris.Contains(extensionUri);
		}

		/// <summary>
		/// Determines whether a given extension is supported by this endpoint.
		/// </summary>
		/// <param name="extension">An instance of the extension to check support for.</param>
		/// <returns>
		/// 	<c>true</c> if the extension is supported by this endpoint; otherwise, <c>false</c>.
		/// </returns>
		public bool IsExtensionSupported(IOpenIdMessageExtension extension) {
			ErrorUtilities.VerifyArgumentNotNull(extension, "extension");

			// Consider the primary case.
			if (this.IsExtensionSupported(extension.TypeUri)) {
				return true;
			}

			// Consider the secondary cases.
			if (extension.AdditionalSupportedTypeUris != null) {
				if (extension.AdditionalSupportedTypeUris.Any(typeUri => this.IsExtensionSupported(typeUri))) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether a given extension is supported by this endpoint.
		/// </summary>
		/// <typeparam name="T">The type of extension to check support for on this endpoint.</typeparam>
		/// <returns>
		/// 	<c>true</c> if the extension is supported by this endpoint; otherwise, <c>false</c>.
		/// </returns>
		public bool IsExtensionSupported<T>() where T : IOpenIdMessageExtension, new() {
			T extension = new T();
			return this.IsExtensionSupported(extension);
		}

		/// <summary>
		/// Determines whether a given extension is supported by this endpoint.
		/// </summary>
		/// <param name="extensionType">The type of extension to check support for on this endpoint.</param>
		/// <returns>
		/// 	<c>true</c> if the extension is supported by this endpoint; otherwise, <c>false</c>.
		/// </returns>
		public bool IsExtensionSupported(Type extensionType) {
			ErrorUtilities.VerifyArgumentNotNull(extensionType, "extensionType");
			ErrorUtilities.VerifyArgument(typeof(IOpenIdMessageExtension).IsAssignableFrom(extensionType), OpenIdStrings.TypeMustImplementX, typeof(IOpenIdMessageExtension).FullName);
			var extension = (IOpenIdMessageExtension)Activator.CreateInstance(extensionType);
			return this.IsExtensionSupported(extension);
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			ServiceEndpoint other = obj as ServiceEndpoint;
			if (other == null) {
				return false;
			}

			// We specifically do not check our ProviderSupportedServiceTypeUris array
			// or the priority field
			// as that is not persisted in our tokens, and it is not part of the 
			// important assertion validation that is part of the spec.
			return
				this.ClaimedIdentifier == other.ClaimedIdentifier &&
				this.ProviderEndpoint == other.ProviderEndpoint &&
				this.ProviderLocalIdentifier == other.ProviderLocalIdentifier &&
				this.Protocol.EqualsPractically(other.Protocol);
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			return this.ClaimedIdentifier.GetHashCode();
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString() {
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("ClaimedIdentifier: " + this.ClaimedIdentifier);
			builder.AppendLine("ProviderLocalIdentifier: " + this.ProviderLocalIdentifier);
			builder.AppendLine("ProviderEndpoint: " + this.ProviderEndpoint.AbsoluteUri);
			builder.AppendLine("OpenID version: " + this.Protocol.Version);
			builder.AppendLine("Service Type URIs:");
			if (this.ProviderSupportedServiceTypeUris != null) {
				foreach (string serviceTypeUri in this.ProviderSupportedServiceTypeUris) {
					builder.Append("\t");
					builder.AppendLine(serviceTypeUri);
				}
			} else {
				builder.AppendLine("\t(unavailable)");
			}
			builder.Length -= Environment.NewLine.Length; // trim last newline
			return builder.ToString();
		}

		/// <summary>
		/// Reads previously discovered information about an endpoint
		/// from a solicited authentication assertion for validation.
		/// </summary>
		/// <param name="reader">The reader from which to deserialize the <see cref="ServiceEndpoint"/>.</param>
		/// <returns>
		/// A <see cref="ServiceEndpoint"/> object that has everything
		/// except the <see cref="ProviderSupportedServiceTypeUris"/>
		/// deserialized.
		/// </returns>
		internal static ServiceEndpoint Deserialize(TextReader reader) {
			var claimedIdentifier = Identifier.Parse(reader.ReadLine());
			var providerLocalIdentifier = Identifier.Parse(reader.ReadLine());
			string userSuppliedIdentifier = reader.ReadLine();
			if (userSuppliedIdentifier.Length == 0) {
				userSuppliedIdentifier = null;
			}
			var providerEndpoint = new Uri(reader.ReadLine());
			var protocol = Protocol.FindBestVersion(p => p.Version, new[] { new Version(reader.ReadLine()) });
			return new ServiceEndpoint(providerEndpoint, claimedIdentifier, userSuppliedIdentifier, providerLocalIdentifier, protocol);
		}

		/// <summary>
		/// Creates a <see cref="ServiceEndpoint"/> instance to represent some OP Identifier.
		/// </summary>
		/// <param name="providerIdentifier">The provider identifier.</param>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="servicePriority">The service priority.</param>
		/// <param name="uriPriority">The URI priority.</param>
		/// <returns>The created <see cref="ServiceEndpoint"/> instance</returns>
		internal static ServiceEndpoint CreateForProviderIdentifier(Identifier providerIdentifier, ProviderEndpointDescription providerEndpoint, int? servicePriority, int? uriPriority) {
			ErrorUtilities.VerifyArgumentNotNull(providerEndpoint, "providerEndpoint");

			Protocol protocol = Protocol.Detect(providerEndpoint.Capabilities);

			return new ServiceEndpoint(
				providerEndpoint,
				protocol.ClaimedIdentifierForOPIdentifier,
				providerIdentifier,
				protocol.ClaimedIdentifierForOPIdentifier,
				servicePriority,
				uriPriority);
		}

		/// <summary>
		/// Creates a <see cref="ServiceEndpoint"/> instance to represent some Claimed Identifier.
		/// </summary>
		/// <param name="claimedIdentifier">The claimed identifier.</param>
		/// <param name="providerLocalIdentifier">The provider local identifier.</param>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="servicePriority">The service priority.</param>
		/// <param name="uriPriority">The URI priority.</param>
		/// <returns>The created <see cref="ServiceEndpoint"/> instance</returns>
		internal static ServiceEndpoint CreateForClaimedIdentifier(Identifier claimedIdentifier, Identifier providerLocalIdentifier, ProviderEndpointDescription providerEndpoint, int? servicePriority, int? uriPriority) {
			return CreateForClaimedIdentifier(claimedIdentifier, null, providerLocalIdentifier, providerEndpoint, servicePriority, uriPriority);
		}

		/// <summary>
		/// Creates a <see cref="ServiceEndpoint"/> instance to represent some Claimed Identifier.
		/// </summary>
		/// <param name="claimedIdentifier">The claimed identifier.</param>
		/// <param name="userSuppliedIdentifier">The user supplied identifier.</param>
		/// <param name="providerLocalIdentifier">The provider local identifier.</param>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="servicePriority">The service priority.</param>
		/// <param name="uriPriority">The URI priority.</param>
		/// <returns>The created <see cref="ServiceEndpoint"/> instance</returns>
		internal static ServiceEndpoint CreateForClaimedIdentifier(Identifier claimedIdentifier, Identifier userSuppliedIdentifier, Identifier providerLocalIdentifier, ProviderEndpointDescription providerEndpoint, int? servicePriority, int? uriPriority) {
			return new ServiceEndpoint(providerEndpoint, claimedIdentifier, userSuppliedIdentifier, providerLocalIdentifier, servicePriority, uriPriority);
		}

		/// <summary>
		/// Saves the discovered information about this endpoint
		/// for later comparison to validate assertions.
		/// </summary>
		/// <param name="writer">The writer to use for serializing out the fields.</param>
		internal void Serialize(TextWriter writer) {
			writer.WriteLine(this.ClaimedIdentifier);
			writer.WriteLine(this.ProviderLocalIdentifier);
			writer.WriteLine(this.UserSuppliedIdentifier);
			writer.WriteLine(this.ProviderEndpoint);
			writer.WriteLine(this.Protocol.Version);

			// No reason to serialize priority. We only needed priority to decide whether to use this endpoint.
		}

		/// <summary>
		/// Gets the priority rating for a given type of endpoint, allowing a
		/// priority sorting of endpoints.
		/// </summary>
		/// <param name="endpoint">The endpoint to prioritize.</param>
		/// <returns>An arbitary integer, which may be used for sorting against other returned values from this method.</returns>
		private static double GetEndpointPrecedenceOrderByServiceType(IXrdsProviderEndpoint endpoint) {
			// The numbers returned from this method only need to compare against other numbers
			// from this method, which makes them arbitrary but relational to only others here.
			if (endpoint.IsTypeUriPresent(Protocol.V20.OPIdentifierServiceTypeURI)) {
				return 0;
			}
			if (endpoint.IsTypeUriPresent(Protocol.V20.ClaimedIdentifierServiceTypeURI)) {
				return 1;
			}
			if (endpoint.IsTypeUriPresent(Protocol.V11.ClaimedIdentifierServiceTypeURI)) {
				return 2;
			}
			if (endpoint.IsTypeUriPresent(Protocol.V10.ClaimedIdentifierServiceTypeURI)) {
				return 3;
			}
			return 10;
		}
	}
}
