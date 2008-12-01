//-----------------------------------------------------------------------
// <copyright file="ServiceEndpoint.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Represents information discovered about a user-supplied Identifier.
	/// </summary>
	[DebuggerDisplay("ClaimedIdentifier: {ClaimedIdentifier}, ProviderEndpoint: {ProviderEndpoint}, OpenId: {Protocol.Version}")]
	internal class ServiceEndpoint : IXrdsProviderEndpoint {
		/// <summary>
		/// The i-name identifier the user actually typed in
		/// or the url identifier with the scheme stripped off.
		/// </summary>
		private string friendlyIdentifierForDisplay;

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

		/// <remarks>
		/// Used for deserializing <see cref="ServiceEndpoint"/> from authentication responses.
		/// </remarks>
		private ServiceEndpoint(Identifier claimedIdentifier, Identifier userSuppliedIdentifier, Uri providerEndpoint, Identifier providerLocalIdentifier, Protocol protocol) {
			this.ClaimedIdentifier = claimedIdentifier;
			this.UserSuppliedIdentifier = userSuppliedIdentifier;
			this.ProviderDescription = new ProviderEndpointDescription(providerEndpoint, protocol.Version);
			this.ProviderLocalIdentifier = providerLocalIdentifier ?? claimedIdentifier;
			this.protocol = protocol;
		}

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
		/// Gets the Identifier that the end user claims to own.
		/// </summary>
		public Identifier ClaimedIdentifier { get; private set; }

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
						Debug.Fail("Doh!  We never should have reached here.");
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
					this.protocol = Protocol.Detect(this.ProviderSupportedServiceTypeUris);
				}
				if (this.protocol != null) {
					return this.protocol;
				}
				throw new InvalidOperationException("Unable to determine the version of OpenID the Provider supports.");
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

		Version IProviderEndpoint.Version { get { return Protocol.Version; } }

		/// <summary>
		/// Gets a value indicating whether the <see cref="ProviderEndpoint"/> is using an encrypted channel.
		/// </summary>
		internal bool IsSecure {
			get { return string.Equals(this.ProviderEndpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase); }
		}

		internal ProviderEndpointDescription ProviderDescription { get; set; }

		public static bool operator ==(ServiceEndpoint se1, ServiceEndpoint se2) {
			return se1.EqualsNullSafe(se2);
		}

		public static bool operator !=(ServiceEndpoint se1, ServiceEndpoint se2) {
			return !(se1 == se2);
		}

		public bool IsTypeUriPresent(string typeUri) {
			return this.IsExtensionSupported(typeUri);
		}

		public bool IsExtensionSupported(string extensionUri) {
			if (this.ProviderSupportedServiceTypeUris == null) {
				throw new InvalidOperationException("Cannot lookup extension support on a rehydrated ServiceEndpoint.");
			}
			return this.ProviderSupportedServiceTypeUris.Contains(extensionUri);
		}

		////public bool IsExtensionSupported(IExtension extension) {
		////    if (extension == null) throw new ArgumentNullException("extension");

		////    // Consider the primary case.
		////    if (IsExtensionSupported(extension.TypeUri)) {
		////        return true;
		////    }
		////    // Consider the secondary cases.
		////    if (extension.AdditionalSupportedTypeUris != null) {
		////        foreach (string extensionTypeUri in extension.AdditionalSupportedTypeUris) {
		////            if (IsExtensionSupported(extensionTypeUri)) {
		////                return true;
		////            }
		////        }
		////    }
		////    return false;
		////}

		////public bool IsExtensionSupported<T>() where T : Extensions.IExtension, new() {
		////    T extension = new T();
		////    return IsExtensionSupported(extension);
		////}

		////public bool IsExtensionSupported(Type extensionType) {
		////    if (extensionType == null) throw new ArgumentNullException("extensionType");
		////    if (!typeof(Extensions.IExtension).IsAssignableFrom(extensionType))
		////        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
		////            Strings.TypeMustImplementX, typeof(Extensions.IExtension).FullName),
		////            "extensionType");
		////    var extension = (Extensions.IExtension)Activator.CreateInstance(extensionType);
		////    return IsExtensionSupported(extension);
		////}

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
				this.Protocol == other.Protocol;
		}

		public override int GetHashCode() {
			return this.ClaimedIdentifier.GetHashCode();
		}

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
					//// TODO: uncomment when we support extensions
					////var matchingExtension = Util.FirstOrDefault(ExtensionManager.RequestExtensions, ext => ext.Key.TypeUri == serviceTypeUri);
					////if (matchingExtension.Key != null) {
					////    builder.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0} ({1})", serviceTypeUri, matchingExtension.Value));
					////} else {
					////    builder.AppendLine(serviceTypeUri);
					////}
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
			return new ServiceEndpoint(claimedIdentifier, userSuppliedIdentifier, providerEndpoint, providerLocalIdentifier, protocol);
		}

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

		internal static ServiceEndpoint CreateForClaimedIdentifier(Identifier claimedIdentifier, Identifier providerLocalIdentifier, ProviderEndpointDescription providerEndpoint, int? servicePriority, int? uriPriority) {
			return CreateForClaimedIdentifier(claimedIdentifier, null, providerLocalIdentifier, providerEndpoint, servicePriority, uriPriority);
		}

		internal static ServiceEndpoint CreateForClaimedIdentifier(Identifier claimedIdentifier, Identifier userSuppliedIdentifier, Identifier providerLocalIdentifier, ProviderEndpointDescription providerEndpoint, int? servicePriority, int? uriPriority) {
			return new ServiceEndpoint(providerEndpoint, claimedIdentifier, userSuppliedIdentifier, providerLocalIdentifier, servicePriority, uriPriority);
		}

		/// <summary>
		/// Saves the discovered information about this endpoint
		/// for later comparison to validate assertions.
		/// </summary>
		internal void Serialize(TextWriter writer) {
			writer.WriteLine(this.ClaimedIdentifier);
			writer.WriteLine(this.ProviderLocalIdentifier);
			writer.WriteLine(this.UserSuppliedIdentifier);
			writer.WriteLine(this.ProviderEndpoint);
			writer.WriteLine(this.Protocol.Version);

			// No reason to serialize priority. We only needed priority to decide whether to use this endpoint.
		}
	}
}