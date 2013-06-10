//-----------------------------------------------------------------------
// <copyright file="Protocol.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// An enumeration of the OAuth protocol versions supported by this library.
	/// </summary>
	public enum ProtocolVersion {
		/// <summary>
		/// OAuth 1.0 specification
		/// </summary>
		V10,

		/// <summary>
		/// OAuth 1.0a specification
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "a", Justification = "By design.")]
		V10a,
	}

	/// <summary>
	/// Constants used in the OAuth protocol.
	/// </summary>
	/// <remarks>
	/// OAuth Protocol Parameter names and values are case sensitive. Each OAuth Protocol Parameters MUST NOT appear more than once per request, and are REQUIRED unless otherwise noted,
	/// per OAuth 1.0 section 5.
	/// </remarks>
	[DebuggerDisplay("OAuth {Version}")]
	internal class Protocol {
		/// <summary>
		/// The namespace to use for V1.0 of the protocol.
		/// </summary>
		internal const string DataContractNamespaceV10 = "http://oauth.net/core/1.0/";

		/// <summary>
		/// The prefix used for all key names in the protocol.
		/// </summary>
		internal const string ParameterPrefix = "oauth_";

		/// <summary>
		/// The string representation of a <see cref="Version"/> instance to be used to represent OAuth 1.0a.
		/// </summary>
		internal const string V10aVersion = "1.0.1";

		/// <summary>
		/// The scheme to use in Authorization header message requests.
		/// </summary>
		internal const string AuthorizationHeaderScheme = "OAuth";

		/// <summary>
		/// The name of the 'oauth_callback' parameter.
		/// </summary>
		internal const string CallbackParameter = "oauth_callback";

		/// <summary>
		/// The name of the 'oauth_callback_confirmed' parameter.
		/// </summary>
		internal const string CallbackConfirmedParameter = "oauth_callback_confirmed";

		/// <summary>
		/// The name of the 'oauth_token' parameter.
		/// </summary>
		internal const string TokenParameter = "oauth_token";

		/// <summary>
		/// The name of the 'oauth_token_secret' parameter.
		/// </summary>
		internal const string TokenSecretParameter = "oauth_token_secret";

		/// <summary>
		/// The name of the 'oauth_verifier' parameter.
		/// </summary>
		internal const string VerifierParameter = "oauth_verifier";

		/// <summary>
		/// Gets the <see cref="Protocol"/> instance with values initialized for V1.0 of the protocol.
		/// </summary>
		internal static readonly Protocol V10 = new Protocol {
			dataContractNamespace = DataContractNamespaceV10,
			Version = new Version(1, 0),
			ProtocolVersion = ProtocolVersion.V10,
		};

		/// <summary>
		/// Gets the <see cref="Protocol"/> instance with values initialized for V1.0a of the protocol.
		/// </summary>
		internal static readonly Protocol V10a = new Protocol {
			dataContractNamespace = DataContractNamespaceV10,
			Version = new Version(V10aVersion),
			ProtocolVersion = ProtocolVersion.V10a,
		};

		/// <summary>
		/// A list of all supported OAuth versions, in order starting from newest version.
		/// </summary>
		internal static readonly List<Protocol> AllVersions = new List<Protocol>() { V10a, V10 };

		/// <summary>
		/// The default (or most recent) supported version of the OAuth protocol.
		/// </summary>
		internal static readonly Protocol Default = AllVersions[0];

		/// <summary>
		/// The namespace to use for this version of the protocol.
		/// </summary>
		private string dataContractNamespace;

		/// <summary>
		/// Initializes a new instance of the <see cref="Protocol"/> class.
		/// </summary>
		internal Protocol() {
			this.PublishedVersion = "1.0";
		}

		/// <summary>
		/// Gets the OAuth version this instance represents.
		/// </summary>
		internal Version Version { get; private set; }

		/// <summary>
		/// Gets the version to declare on the wire.
		/// </summary>
		internal string PublishedVersion { get; private set; }

		/// <summary>
		/// Gets the <see cref="ProtocolVersion"/> enum value for the <see cref="Protocol"/> instance.
		/// </summary>
		internal ProtocolVersion ProtocolVersion { get; private set; }

		/// <summary>
		/// Gets the namespace to use for this version of the protocol.
		/// </summary>
		internal string DataContractNamespace {
			get { return this.dataContractNamespace; }
		}

		/// <summary>
		/// Gets the OAuth Protocol instance to use for the given version.
		/// </summary>
		/// <param name="version">The OAuth version to get.</param>
		/// <returns>A matching <see cref="Protocol"/> instance.</returns>
		public static Protocol Lookup(ProtocolVersion version) {
			switch (version) {
				case ProtocolVersion.V10: return Protocol.V10;
				case ProtocolVersion.V10a: return Protocol.V10a;
				default: throw new ArgumentOutOfRangeException("version");
			}
		}

		/// <summary>
		/// Gets the OAuth Protocol instance to use for the given version.
		/// </summary>
		/// <param name="version">The OAuth version to get.</param>
		/// <returns>A matching <see cref="Protocol"/> instance.</returns>
		internal static Protocol Lookup(Version version) {
			Requires.NotNull(version, "version");
			Requires.Range(AllVersions.Any(p => p.Version == version), "version");
			return AllVersions.First(p => p.Version == version);
		}
	}
}
