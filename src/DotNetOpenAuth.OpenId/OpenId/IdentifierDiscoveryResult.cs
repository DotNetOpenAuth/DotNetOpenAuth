//-----------------------------------------------------------------------
// <copyright file="IdentifierDiscoveryResult.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Validation;

	/// <summary>
	/// Represents a single OP endpoint from discovery on some OpenID Identifier.
	/// </summary>
	[DebuggerDisplay("ClaimedIdentifier: {ClaimedIdentifier}, ProviderEndpoint: {ProviderEndpoint}, OpenId: {Protocol.Version}")]
	public sealed class IdentifierDiscoveryResult : IProviderEndpoint {
		/// <summary>
		/// Backing field for the <see cref="Protocol"/> property.
		/// </summary>
		private Protocol protocol;

		/// <summary>
		/// Backing field for the <see cref="ClaimedIdentifier"/> property.
		/// </summary>
		private Identifier claimedIdentifier;

		/// <summary>
		/// Backing field for the <see cref="FriendlyIdentifierForDisplay"/> property.
		/// </summary>
		private string friendlyIdentifierForDisplay;

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentifierDiscoveryResult"/> class.
		/// </summary>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="claimedIdentifier">The Claimed Identifier.</param>
		/// <param name="userSuppliedIdentifier">The User-supplied Identifier.</param>
		/// <param name="providerLocalIdentifier">The Provider Local Identifier.</param>
		/// <param name="servicePriority">The service priority.</param>
		/// <param name="uriPriority">The URI priority.</param>
		private IdentifierDiscoveryResult(ProviderEndpointDescription providerEndpoint, Identifier claimedIdentifier, Identifier userSuppliedIdentifier, Identifier providerLocalIdentifier, int? servicePriority, int? uriPriority) {
			Requires.NotNull(providerEndpoint, "providerEndpoint");
			Requires.NotNull(claimedIdentifier, "claimedIdentifier");
			this.ProviderEndpoint = providerEndpoint.Uri;
			this.Capabilities = new ReadOnlyCollection<string>(providerEndpoint.Capabilities);
			this.Version = providerEndpoint.Version;
			this.ClaimedIdentifier = claimedIdentifier;
			this.ProviderLocalIdentifier = providerLocalIdentifier ?? claimedIdentifier;
			this.UserSuppliedIdentifier = userSuppliedIdentifier;
			this.ServicePriority = servicePriority;
			this.ProviderEndpointPriority = uriPriority;
		}

		/// <summary>
		/// Gets the detected version of OpenID implemented by the Provider.
		/// </summary>
		public Version Version { get; private set; }

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
		/// Gets the Identifier that the end user claims to control.
		/// </summary>
		public Identifier ClaimedIdentifier {
			get {
				return this.claimedIdentifier;
			}

			internal set {
				// Take care to reparse the incoming identifier to make sure it's
				// not a derived type that will override expected behavior.
				// Elsewhere in this class, we count on the fact that this property
				// is either UriIdentifier or XriIdentifier.  MockIdentifier messes it up.
				this.claimedIdentifier = value != null ? Identifier.Reparse(value) : null;
			}
		}

		/// <summary>
		/// Gets an alternate Identifier for an end user that is local to a 
		/// particular OP and thus not necessarily under the end user's 
		/// control.
		/// </summary>
		public Identifier ProviderLocalIdentifier { get; private set; }

		/// <summary>
		/// Gets a more user-friendly (but NON-secure!) string to display to the user as his identifier.
		/// </summary>
		/// <returns>A human-readable, abbreviated (but not secure) identifier the user MAY recognize as his own.</returns>
		public string FriendlyIdentifierForDisplay {
			get {
				if (this.friendlyIdentifierForDisplay == null) {
					XriIdentifier xri = this.ClaimedIdentifier as XriIdentifier;
					UriIdentifier uri = this.ClaimedIdentifier as UriIdentifier;
					if (xri != null) {
						if (this.UserSuppliedIdentifier == null || string.Equals(this.UserSuppliedIdentifier, this.ClaimedIdentifier, StringComparison.OrdinalIgnoreCase)) {
							this.friendlyIdentifierForDisplay = this.ClaimedIdentifier;
						} else {
							this.friendlyIdentifierForDisplay = this.UserSuppliedIdentifier;
						}
					} else if (uri != null) {
						if (uri != this.Protocol.ClaimedIdentifierForOPIdentifier) {
							string displayUri = uri.Uri.Host;

							// We typically want to display the path, because that will often have the username in it.
							// As Google Apps for Domains and the like become more popular, a standard /openid path
							// will often appear, which is not helpful to identifying the user so we'll avoid including
							// that path if it's present.
							if (!string.Equals(uri.Uri.AbsolutePath, "/openid", StringComparison.OrdinalIgnoreCase)) {
								displayUri += uri.Uri.AbsolutePath.TrimEnd('/');
							}

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
		/// Gets the provider endpoint.
		/// </summary>
		public Uri ProviderEndpoint { get; private set; }

		/// <summary>
		/// Gets the @priority given in the XRDS document for this specific OP endpoint.
		/// </summary>
		public int? ProviderEndpointPriority { get; private set; }

		/// <summary>
		/// Gets the @priority given in the XRDS document for this service
		/// (which may consist of several endpoints).
		/// </summary>
		public int? ServicePriority { get; private set; }

		/// <summary>
		/// Gets the collection of service type URIs found in the XRDS document describing this Provider.
		/// </summary>
		/// <value>Should never be null, but may be empty.</value>
		public ReadOnlyCollection<string> Capabilities { get; private set; }

		#region IProviderEndpoint Members

		/// <summary>
		/// Gets the URL that the OpenID Provider receives authentication requests at.
		/// </summary>
		/// <value>This value MUST be an absolute HTTP or HTTPS URL.</value>
		Uri IProviderEndpoint.Uri {
			get { return this.ProviderEndpoint; }
		}

		#endregion

		/// <summary>
		/// Gets an XRDS sorting routine that uses the XRDS Service/@Priority 
		/// attribute to determine order.
		/// </summary>
		/// <remarks>
		/// Endpoints lacking any priority value are sorted to the end of the list.
		/// </remarks>
		internal static Comparison<IdentifierDiscoveryResult> EndpointOrder {
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
						if (se1.ProviderEndpointPriority.HasValue && se2.ProviderEndpointPriority.HasValue) {
							return se1.ProviderEndpointPriority.Value.CompareTo(se2.ProviderEndpointPriority.Value);
						} else if (se1.ProviderEndpointPriority.HasValue) {
							return -1;
						} else if (se2.ProviderEndpointPriority.HasValue) {
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
							if (se1.ProviderEndpointPriority.HasValue && se2.ProviderEndpointPriority.HasValue) {
								return se1.ProviderEndpointPriority.Value.CompareTo(se2.ProviderEndpointPriority.Value);
							} else if (se1.ProviderEndpointPriority.HasValue) {
								return -1;
							} else if (se2.ProviderEndpointPriority.HasValue) {
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
		/// Gets the protocol used by the OpenID Provider.
		/// </summary>
		internal Protocol Protocol {
			get {
				if (this.protocol == null) {
					this.protocol = Protocol.Lookup(this.Version);
				}

				return this.protocol;
			}
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="se1">The first service endpoint.</param>
		/// <param name="se2">The second service endpoint.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(IdentifierDiscoveryResult se1, IdentifierDiscoveryResult se2) {
			return se1.EqualsNullSafe(se2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="se1">The first service endpoint.</param>
		/// <param name="se2">The second service endpoint.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(IdentifierDiscoveryResult se1, IdentifierDiscoveryResult se2) {
			return !(se1 == se2);
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
			var other = obj as IdentifierDiscoveryResult;
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
			builder.AppendLine("ProviderEndpoint: " + this.ProviderEndpoint);
			builder.AppendLine("OpenID version: " + this.Version);
			builder.AppendLine("Service Type URIs:");
			foreach (string serviceTypeUri in this.Capabilities) {
				builder.Append("\t");
				builder.AppendLine(serviceTypeUri);
			}
			builder.Length -= Environment.NewLine.Length; // trim last newline
			return builder.ToString();
		}

		/// <summary>
		/// Checks whether the OpenId Identifier claims support for a given extension.
		/// </summary>
		/// <typeparam name="T">The extension whose support is being queried.</typeparam>
		/// <returns>
		/// True if support for the extension is advertised.  False otherwise.
		/// </returns>
		/// <remarks>
		/// Note that a true or false return value is no guarantee of a Provider's
		/// support for or lack of support for an extension.  The return value is
		/// determined by how the authenticating user filled out his/her XRDS document only.
		/// The only way to be sure of support for a given extension is to include
		/// the extension in the request and see if a response comes back for that extension.
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "No parameter at all.")]
		public bool IsExtensionSupported<T>() where T : IOpenIdMessageExtension, new() {
			T extension = new T();
			return this.IsExtensionSupported(extension);
		}

		/// <summary>
		/// Checks whether the OpenId Identifier claims support for a given extension.
		/// </summary>
		/// <param name="extensionType">The extension whose support is being queried.</param>
		/// <returns>
		/// True if support for the extension is advertised.  False otherwise.
		/// </returns>
		/// <remarks>
		/// Note that a true or false return value is no guarantee of a Provider's
		/// support for or lack of support for an extension.  The return value is
		/// determined by how the authenticating user filled out his/her XRDS document only.
		/// The only way to be sure of support for a given extension is to include
		/// the extension in the request and see if a response comes back for that extension.
		/// </remarks>
		public bool IsExtensionSupported(Type extensionType) {
			var extension = (IOpenIdMessageExtension)Activator.CreateInstance(extensionType);
			return this.IsExtensionSupported(extension);
		}

		/// <summary>
		/// Determines whether a given extension is supported by this endpoint.
		/// </summary>
		/// <param name="extension">An instance of the extension to check support for.</param>
		/// <returns>
		/// 	<c>true</c> if the extension is supported by this endpoint; otherwise, <c>false</c>.
		/// </returns>
		public bool IsExtensionSupported(IOpenIdMessageExtension extension) {
			Requires.NotNull(extension, "extension");

			// Consider the primary case.
			if (this.IsTypeUriPresent(extension.TypeUri)) {
				return true;
			}

			// Consider the secondary cases.
			if (extension.AdditionalSupportedTypeUris != null) {
				if (extension.AdditionalSupportedTypeUris.Any(typeUri => this.IsTypeUriPresent(typeUri))) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Creates a <see cref="IdentifierDiscoveryResult"/> instance to represent some OP Identifier.
		/// </summary>
		/// <param name="providerIdentifier">The provider identifier (actually the user-supplied identifier).</param>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="servicePriority">The service priority.</param>
		/// <param name="uriPriority">The URI priority.</param>
		/// <returns>The created <see cref="IdentifierDiscoveryResult"/> instance</returns>
		internal static IdentifierDiscoveryResult CreateForProviderIdentifier(Identifier providerIdentifier, ProviderEndpointDescription providerEndpoint, int? servicePriority, int? uriPriority) {
			Requires.NotNull(providerEndpoint, "providerEndpoint");

			Protocol protocol = Protocol.Lookup(providerEndpoint.Version);

			return new IdentifierDiscoveryResult(
				providerEndpoint,
				protocol.ClaimedIdentifierForOPIdentifier,
				providerIdentifier,
				protocol.ClaimedIdentifierForOPIdentifier,
				servicePriority,
				uriPriority);
		}

		/// <summary>
		/// Creates a <see cref="IdentifierDiscoveryResult"/> instance to represent some Claimed Identifier.
		/// </summary>
		/// <param name="claimedIdentifier">The claimed identifier.</param>
		/// <param name="providerLocalIdentifier">The provider local identifier.</param>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="servicePriority">The service priority.</param>
		/// <param name="uriPriority">The URI priority.</param>
		/// <returns>The created <see cref="IdentifierDiscoveryResult"/> instance</returns>
		internal static IdentifierDiscoveryResult CreateForClaimedIdentifier(Identifier claimedIdentifier, Identifier providerLocalIdentifier, ProviderEndpointDescription providerEndpoint, int? servicePriority, int? uriPriority) {
			return CreateForClaimedIdentifier(claimedIdentifier, null, providerLocalIdentifier, providerEndpoint, servicePriority, uriPriority);
		}

		/// <summary>
		/// Creates a <see cref="IdentifierDiscoveryResult"/> instance to represent some Claimed Identifier.
		/// </summary>
		/// <param name="claimedIdentifier">The claimed identifier.</param>
		/// <param name="userSuppliedIdentifier">The user supplied identifier.</param>
		/// <param name="providerLocalIdentifier">The provider local identifier.</param>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="servicePriority">The service priority.</param>
		/// <param name="uriPriority">The URI priority.</param>
		/// <returns>The created <see cref="IdentifierDiscoveryResult"/> instance</returns>
		internal static IdentifierDiscoveryResult CreateForClaimedIdentifier(Identifier claimedIdentifier, Identifier userSuppliedIdentifier, Identifier providerLocalIdentifier, ProviderEndpointDescription providerEndpoint, int? servicePriority, int? uriPriority) {
			return new IdentifierDiscoveryResult(providerEndpoint, claimedIdentifier, userSuppliedIdentifier, providerLocalIdentifier, servicePriority, uriPriority);
		}

		/// <summary>
		/// Determines whether a given type URI is present on the specified provider endpoint.
		/// </summary>
		/// <param name="typeUri">The type URI.</param>
		/// <returns>
		/// 	<c>true</c> if the type URI is present on the specified provider endpoint; otherwise, <c>false</c>.
		/// </returns>
		internal bool IsTypeUriPresent(string typeUri) {
			Requires.NotNullOrEmpty(typeUri, "typeUri");
			return this.Capabilities.Contains(typeUri);
		}

		/// <summary>
		/// Sets the Capabilities property (this method is a test hook.)
		/// </summary>
		/// <param name="value">The value.</param>
		/// <remarks>The publicize.exe tool should work for the unit tests, but for some reason it fails on the build server.</remarks>
		internal void SetCapabilitiesForTestHook(ReadOnlyCollection<string> value) {
			this.Capabilities = value;
		}

		/// <summary>
		/// Gets the priority rating for a given type of endpoint, allowing a
		/// priority sorting of endpoints.
		/// </summary>
		/// <param name="endpoint">The endpoint to prioritize.</param>
		/// <returns>An arbitary integer, which may be used for sorting against other returned values from this method.</returns>
		private static double GetEndpointPrecedenceOrderByServiceType(IdentifierDiscoveryResult endpoint) {
			// The numbers returned from this method only need to compare against other numbers
			// from this method, which makes them arbitrary but relational to only others here.
			if (endpoint.Capabilities.Contains(Protocol.V20.OPIdentifierServiceTypeURI)) {
				return 0;
			}
			if (endpoint.Capabilities.Contains(Protocol.V20.ClaimedIdentifierServiceTypeURI)) {
				return 1;
			}
			if (endpoint.Capabilities.Contains(Protocol.V11.ClaimedIdentifierServiceTypeURI)) {
				return 2;
			}
			if (endpoint.Capabilities.Contains(Protocol.V10.ClaimedIdentifierServiceTypeURI)) {
				return 3;
			}
			return 10;
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Called by code contracts.")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.ProviderEndpoint != null);
			Contract.Invariant(this.ClaimedIdentifier != null);
			Contract.Invariant(this.ProviderLocalIdentifier != null);
			Contract.Invariant(this.Capabilities != null);
			Contract.Invariant(this.Version != null);
			Contract.Invariant(this.Protocol != null);
		}
#endif
	}
}
