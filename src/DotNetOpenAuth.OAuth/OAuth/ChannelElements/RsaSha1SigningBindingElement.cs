//-----------------------------------------------------------------------
// <copyright file="RsaSha1SigningBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A binding element that signs outgoing messages and verifies the signature on incoming messages.
	/// </summary>
	public abstract class RsaSha1SigningBindingElement : SigningBindingElementBase {
		/// <summary>
		/// The name of the hash algorithm to use.
		/// </summary>
		protected const string HashAlgorithmName = "RSA-SHA1";

		/// <summary>
		/// Initializes a new instance of the <see cref="RsaSha1SigningBindingElement"/> class.
		/// </summary>
		protected RsaSha1SigningBindingElement()
			: base(HashAlgorithmName) {
		}
	}
}
