//-----------------------------------------------------------------------
// <copyright file="Protocol.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Constants used in the OAuth protocol.
	/// </summary>
	/// <remarks>
	/// OAuth Protocol Parameter names and values are case sensitive. Each OAuth Protocol Parameters MUST NOT appear more than once per request, and are REQUIRED unless otherwise noted,
	/// per OAuth 1.0 section 5.
	/// </remarks>
	internal class Protocol {
		/// <summary>
		/// The namespace to use for V1.0 of the protocol.
		/// </summary>
		internal const string DataContractNamespaceV10 = "http://oauth.net/core/1.0/";

		/// <summary>
		/// Gets the <see cref="Protocol"/> instance with values initialized for V1.0 of the protocol.
		/// </summary>
		internal static readonly Protocol V10 = new Protocol {
			dataContractNamespace = DataContractNamespaceV10,
		};

		/// <summary>
		/// The namespace to use for this version of the protocol.
		/// </summary>
		private string dataContractNamespace;

		/// <summary>
		/// The prefix used for all key names in the protocol.
		/// </summary>
		private string parameterPrefix = "oauth_";

		/// <summary>
		/// The scheme to use in Authorization header message requests.
		/// </summary>
		private string authorizationHeaderScheme = "OAuth";

		/// <summary>
		/// Gets the default <see cref="Protocol"/> instance.
		/// </summary>
		internal static Protocol Default { get { return V10; } }

		/// <summary>
		/// Gets the namespace to use for this version of the protocol.
		/// </summary>
		internal string DataContractNamespace {
			get { return this.dataContractNamespace; }
		}

		/// <summary>
		/// Gets the prefix used for all key names in the protocol.
		/// </summary>
		internal string ParameterPrefix {
			get { return this.parameterPrefix; }
		}

		/// <summary>
		/// Gets the scheme to use in Authorization header message requests.
		/// </summary>
		internal string AuthorizationHeaderScheme {
			get { return this.authorizationHeaderScheme; }
		}

		/// <summary>
		/// Gets an instance of <see cref="Protocol"/> given a <see cref="Version"/>.
		/// </summary>
		/// <param name="version">The version of the protocol that is desired.</param>
		/// <returns>The <see cref="Protocol"/> instance representing the requested version.</returns>
		internal static Protocol Lookup(Version version) {
			switch (version.Major) {
				case 1: return Protocol.V10;
				default: throw new ArgumentOutOfRangeException("version");
			}
		}
	}
}
