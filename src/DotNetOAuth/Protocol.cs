//-----------------------------------------------------------------------
// <copyright file="Protocol.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Constants used in the OAuth protocol.
	/// </summary>
	/// <remarks>
	/// OAuth Protocol Parameter names and values are case sensitive. Each OAuth Protocol Parameters MUST NOT appear more than once per request, and are REQUIRED unless otherwise noted,
	/// per OAuth 1.0 section 5.
	/// </remarks>
	internal class Protocol {
		/// <summary>
		/// Gets the default <see cref="Protocol"/> instance.
		/// </summary>
		internal static readonly Protocol Default = V10;

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
		/// Strings that identify the various message schemes.
		/// </summary>
		/// <remarks>
		/// These strings should be checked with case INsensitivity.
		/// </remarks>
		private Dictionary<MessageScheme, string> messageSchemes = new Dictionary<MessageScheme, string> {
			{ MessageScheme.AuthorizationHeaderRequest, "OAuth" },
		};

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
		/// Gets the strings that identify the various message schemes.
		/// </summary>
		/// <remarks>
		/// These strings should be checked with case INsensitivity.
		/// </remarks>
		internal IDictionary<MessageScheme, string> MessageSchemes {
			get { return this.messageSchemes; }
		}
	}
}
