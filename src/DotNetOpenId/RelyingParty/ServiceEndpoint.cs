using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using DotNetOpenId.Yadis;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// Represents information discovered about a user-supplied Identifier.
	/// </summary>
	internal class ServiceEndpoint {
		/// <summary>
		/// The URL which accepts OpenID Authentication protocol messages.
		/// </summary>
		/// <remarks>
		/// Obtained by performing discovery on the User-Supplied Identifier. 
		/// This value MUST be an absolute HTTP or HTTPS URL.
		/// </remarks>
		public Uri ProviderEndpoint { get; private set; }
		/*
		/// <summary>
		/// An Identifier for an OpenID Provider.
		/// </summary>
		public Identifier ProviderIdentifier { get; private set; }
		/// <summary>
		/// An Identifier that was presented by the end user to the Relying Party, 
		/// or selected by the user at the OpenID Provider. 
		/// During the initiation phase of the protocol, an end user may enter 
		/// either their own Identifier or an OP Identifier. If an OP Identifier 
		/// is used, the OP may then assist the end user in selecting an Identifier 
		/// to share with the Relying Party.
		/// </summary>
		public Identifier UserSuppliedIdentifier { get; private set; }*/
		/// <summary>
		/// The Identifier that the end user claims to own.
		/// </summary>
		public Identifier ClaimedIdentifier { get; private set; }
		/// <summary>
		/// An alternate Identifier for an end user that is local to a 
		/// particular OP and thus not necessarily under the end user's 
		/// control.
		/// </summary>
		public Identifier ProviderLocalIdentifier { get; private set; }
		/// <summary>
		/// Gets the list of services available at this OP Endpoint for the
		/// claimed Identifier.
		/// </summary>
		public string[] ProviderSupportedServiceTypeUris { get; private set; }

		internal ServiceEndpoint(Identifier claimedIdentifier, Uri providerEndpoint,
			Identifier providerLocalIdentifier, string[] providerSupportedServiceTypeUris) {
			if (claimedIdentifier == null) throw new ArgumentNullException("claimedIdentifier");
			if (providerEndpoint == null) throw new ArgumentNullException("providerEndpoint");
			if (providerSupportedServiceTypeUris == null) throw new ArgumentNullException("providerSupportedServiceTypeUris");
			ClaimedIdentifier = claimedIdentifier;
			ProviderEndpoint = providerEndpoint;
			ProviderLocalIdentifier = providerLocalIdentifier ?? claimedIdentifier;
			ProviderSupportedServiceTypeUris = providerSupportedServiceTypeUris;
		}
		ServiceEndpoint(Identifier claimedIdentifier, Uri providerEndpoint,
			Identifier providerLocalIdentifier, Protocol protocol) {
			ClaimedIdentifier = claimedIdentifier;
			ProviderEndpoint = providerEndpoint;
			ProviderLocalIdentifier = providerLocalIdentifier ?? claimedIdentifier;
			this.protocol = protocol;
		}

		Protocol protocol;
		/// <summary>
		/// Gets the OpenID protocol used by the Provider.
		/// </summary>
		public Protocol Protocol {
			get {
				if (protocol == null) {
					protocol =
						Util.FindBestVersion(p => p.OPIdentifierServiceTypeURI, ProviderSupportedServiceTypeUris) ??
						Util.FindBestVersion(p => p.ClaimedIdentifierServiceTypeURI, ProviderSupportedServiceTypeUris);
				}
				if (protocol != null) return protocol;
				throw new InvalidOperationException("Unable to determine the version of OpenID the Provider supports.");
			}
		}

		public bool IsExtensionSupported(string extensionUri) {
			if (ProviderSupportedServiceTypeUris == null)
				throw new InvalidOperationException("Cannot lookup extension support on a rehydrated ServiceEndpoint.");
			return Array.IndexOf(ProviderSupportedServiceTypeUris, extensionUri) >= 0;
		}

		/// <summary>
		/// Saves the discovered information about this endpoint
		/// for later comparison to validate assertions.
		/// </summary>
		internal void Serialize(TextWriter writer) {
			writer.WriteLine(ClaimedIdentifier);
			writer.WriteLine(ProviderLocalIdentifier);
			writer.WriteLine(ProviderEndpoint);
			writer.WriteLine(Protocol.Version);
		}

		/// <summary>
		/// Reads previously discovered information about an endpoint
		/// from a solicited authentication assertion for validation.
		/// </summary>
		/// <returns>
		/// A <see cref="ServiceEndpoint"/> object that has everything
		/// except the <see cref="ProviderSupportedServiceTypeUris"/>
		/// deserialized.
		/// </returns>
		internal static ServiceEndpoint Deserialize(TextReader reader) {
			var claimedIdentifier = Identifier.Parse(reader.ReadLine());
			var providerLocalIdentifier = Identifier.Parse(reader.ReadLine());
			var providerEndpoint = new Uri(reader.ReadLine());
			var protocol = Util.FindBestVersion(p => p.Version, new[] { new Version(reader.ReadLine()) });
			return new ServiceEndpoint(claimedIdentifier, providerEndpoint,
				providerLocalIdentifier, protocol);
		}

		public override bool Equals(object obj) {
			ServiceEndpoint other = obj as ServiceEndpoint;
			if (other == null) return false;
			// We specifically do not check our ProviderSupportedServiceTypeUris array
			// as that is not persisted in our tokens, and it is not part of the 
			// important assertion validation that is part of the spec.
			return
				this.ClaimedIdentifier == other.ClaimedIdentifier &&
				this.ProviderEndpoint == other.ProviderEndpoint &&
				this.ProviderLocalIdentifier == other.ProviderLocalIdentifier &&
				this.Protocol == other.Protocol;
		}
		public override int GetHashCode() {
			return ClaimedIdentifier.GetHashCode();
		}
	}
}