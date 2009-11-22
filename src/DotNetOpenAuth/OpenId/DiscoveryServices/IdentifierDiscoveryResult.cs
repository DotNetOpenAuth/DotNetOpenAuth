//-----------------------------------------------------------------------
// <copyright file="IdentifierDiscoveryResult.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.DiscoveryServices {
	using System;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.DiscoveryServices;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// Represents a single OP endpoint from discovery on some OpenID Identifier.
	/// </summary>
	[DebuggerDisplay("ClaimedIdentifier: {ClaimedIdentifier}, ProviderEndpoint: {ProviderEndpoint}, OpenId: {Protocol.Version}")]
	internal class IdentifierDiscoveryResult : IIdentifierDiscoveryResult {
		/// <summary>
		/// Backing field for the <see cref="ClaimedIdentifier"/> property.
		/// </summary>
		private Identifier claimedIdentifier;

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
		/// Initializes a new instance of the <see cref="IdentifierDiscoveryResult"/> class.
		/// </summary>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="claimedIdentifier">The Claimed Identifier.</param>
		/// <param name="userSuppliedIdentifier">The User-supplied Identifier.</param>
		/// <param name="providerLocalIdentifier">The Provider Local Identifier.</param>
		/// <param name="servicePriority">The service priority.</param>
		/// <param name="uriPriority">The URI priority.</param>
		private IdentifierDiscoveryResult(IProviderEndpoint providerEndpoint, Identifier claimedIdentifier, Identifier userSuppliedIdentifier, Identifier providerLocalIdentifier, int? servicePriority, int? uriPriority) {
			Contract.Requires<ArgumentNullException>(claimedIdentifier != null);
			Contract.Requires<ArgumentNullException>(providerEndpoint != null);
			this.ProviderEndpoint = providerEndpoint;
			this.ClaimedIdentifier = claimedIdentifier;
			this.UserSuppliedIdentifier = userSuppliedIdentifier;
			this.ProviderLocalIdentifier = providerLocalIdentifier ?? claimedIdentifier;
			this.servicePriority = servicePriority;
			this.uriPriority = uriPriority;
		}

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
		/// Gets or sets the Identifier that the end user claims to control.
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
		/// Gets the provider endpoint.
		/// </summary>
		/// <value>
		/// The discovered provider endpoint.  May optionally implement <see cref="IXrdsProviderEndpoint"/>.
		/// </value>
		public IProviderEndpoint ProviderEndpoint { get; private set; }

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
			var other = obj as IIdentifierDiscoveryResult;
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
				this.ProviderEndpoint.GetProtocol().EqualsPractically(other.ProviderEndpoint.GetProtocol());
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
			builder.AppendLine("ProviderEndpoint: " + this.ProviderEndpoint.Uri.AbsoluteUri);
			builder.AppendLine("OpenID version: " + this.ProviderEndpoint.Version);
			builder.AppendLine("Service Type URIs:");
			foreach (string serviceTypeUri in this.ProviderEndpoint.Capabilities) {
				builder.Append("\t");
				builder.AppendLine(serviceTypeUri);
			}
			builder.Length -= Environment.NewLine.Length; // trim last newline
			return builder.ToString();
		}

		/// <summary>
		/// Creates a <see cref="IIdentifierDiscoveryResult"/> instance to represent some OP Identifier.
		/// </summary>
		/// <param name="providerIdentifier">The provider identifier (actually the user-supplied identifier).</param>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="servicePriority">The service priority.</param>
		/// <param name="uriPriority">The URI priority.</param>
		/// <returns>The created <see cref="IIdentifierDiscoveryResult"/> instance</returns>
		internal static IIdentifierDiscoveryResult CreateForProviderIdentifier(Identifier providerIdentifier, ProviderEndpointDescription providerEndpoint, int? servicePriority, int? uriPriority) {
			Contract.Requires<ArgumentNullException>(providerEndpoint != null);

			Protocol protocol = Protocol.Detect(providerEndpoint.Capabilities);

			return new IdentifierDiscoveryResult(
				providerEndpoint,
				protocol.ClaimedIdentifierForOPIdentifier,
				providerIdentifier,
				protocol.ClaimedIdentifierForOPIdentifier,
				servicePriority,
				uriPriority);
		}

		/// <summary>
		/// Creates a <see cref="IIdentifierDiscoveryResult"/> instance to represent some Claimed Identifier.
		/// </summary>
		/// <param name="claimedIdentifier">The claimed identifier.</param>
		/// <param name="providerLocalIdentifier">The provider local identifier.</param>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="servicePriority">The service priority.</param>
		/// <param name="uriPriority">The URI priority.</param>
		/// <returns>The created <see cref="IIdentifierDiscoveryResult"/> instance</returns>
		internal static IIdentifierDiscoveryResult CreateForClaimedIdentifier(Identifier claimedIdentifier, Identifier providerLocalIdentifier, IProviderEndpoint providerEndpoint, int? servicePriority, int? uriPriority) {
			return CreateForClaimedIdentifier(claimedIdentifier, null, providerLocalIdentifier, providerEndpoint, servicePriority, uriPriority);
		}

		/// <summary>
		/// Creates a <see cref="IIdentifierDiscoveryResult"/> instance to represent some Claimed Identifier.
		/// </summary>
		/// <param name="claimedIdentifier">The claimed identifier.</param>
		/// <param name="userSuppliedIdentifier">The user supplied identifier.</param>
		/// <param name="providerLocalIdentifier">The provider local identifier.</param>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="servicePriority">The service priority.</param>
		/// <param name="uriPriority">The URI priority.</param>
		/// <returns>The created <see cref="IdentifierDiscoveryResult"/> instance</returns>
		internal static IIdentifierDiscoveryResult CreateForClaimedIdentifier(Identifier claimedIdentifier, Identifier userSuppliedIdentifier, Identifier providerLocalIdentifier, IProviderEndpoint providerEndpoint, int? servicePriority, int? uriPriority) {
			return new IdentifierDiscoveryResult(providerEndpoint, claimedIdentifier, userSuppliedIdentifier, providerLocalIdentifier, servicePriority, uriPriority);
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
