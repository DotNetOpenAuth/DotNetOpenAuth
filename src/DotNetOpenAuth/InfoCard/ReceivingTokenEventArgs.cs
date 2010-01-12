//-----------------------------------------------------------------------
// <copyright file="ReceivingTokenEventArgs.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.InfoCard {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.IdentityModel.Tokens;
	using System.Security.Cryptography.X509Certificates;

	/// <summary>
	/// Arguments for the <see cref="InfoCardSelector.ReceivingToken"/> event.
	/// </summary>
	public class ReceivingTokenEventArgs : EventArgs {
		/// <summary>
		/// Initializes a new instance of the <see cref="ReceivingTokenEventArgs"/> class.
		/// </summary>
		/// <param name="tokenXml">The raw token XML, prior to any decryption.</param>
		internal ReceivingTokenEventArgs(string tokenXml) {
			Contract.Requires(tokenXml != null);

			this.TokenXml = tokenXml;
			this.IsEncrypted = Token.IsEncrypted(this.TokenXml);
			this.DecryptingTokens = new List<SecurityToken>();
		}

		/// <summary>
		/// Gets a value indicating whether the token is encrypted.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the token is encrypted; otherwise, <c>false</c>.
		/// </value>
		public bool IsEncrypted { get; private set; }

		/// <summary>
		/// Gets the raw token XML, prior to any decryption.
		/// </summary>
		public string TokenXml { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether processing
		/// this token should be canceled.
		/// </summary>
		/// <value><c>true</c> if cancel; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// If set the <c>true</c>, the <see cref="InfoCardSelector.ReceivedToken"/>
		/// event will never be fired.
		/// </remarks>
		public bool Cancel { get; set; }

		/// <summary>
		/// Gets a list where security tokens such as X.509 certificates may be
		/// added to be used for token decryption.
		/// </summary>
		internal IList<SecurityToken> DecryptingTokens { get; private set; }

		/// <summary>
		/// Adds a security token that may be used to decrypt the incoming token.
		/// </summary>
		/// <param name="securityToken">The security token.</param>
		public void AddDecryptingToken(SecurityToken securityToken) {
			Contract.Requires(securityToken != null);
			this.DecryptingTokens.Add(securityToken);
		}

		/// <summary>
		/// Adds an X.509 certificate with a private key that may be used to decrypt the incoming token.
		/// </summary>
		/// <param name="certificate">The certificate.</param>
		public void AddDecryptingToken(X509Certificate2 certificate) {
			Contract.Requires(certificate != null);
			Contract.Requires(certificate.HasPrivateKey);
			this.AddDecryptingToken(new X509SecurityToken(certificate));
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.TokenXml != null);
			Contract.Invariant(this.DecryptingTokens != null);
		}
#endif
	}
}
