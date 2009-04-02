//-----------------------------------------------------------------------
// <copyright file="ReceivingTokenEventArgs.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.InfoCard {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// Arguments for the <see cref="InfoCardSelector.ReceivingToken"/> event.
	/// </summary>
	public class ReceivingTokenEventArgs : EventArgs {
		/// <summary>
		/// Initializes a new instance of the <see cref="ReceivingTokenEventArgs"/> class.
		/// </summary>
		/// <param name="tokenXml">The raw token XML, prior to any decryption.</param>
		/// <param name="decryptor">The decryptor to use if the token is encrypted.</param>
		internal ReceivingTokenEventArgs(string tokenXml, TokenDecryptor decryptor) {
			Contract.Requires(tokenXml != null);

			this.TokenXml = tokenXml;
			this.IsEncrypted = Token.IsEncrypted(this.TokenXml);
			this.Decryptor = decryptor;
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
		/// Gets the object that will perform token decryption, if necessary.
		/// </summary>
		/// <value>The decryptor to use; or <c>null</c> if the token is not encrypted.</value>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Decryptor", Justification = "By design")]
		public TokenDecryptor Decryptor { get; private set; }

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

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		protected void ObjectInvariant() {
			Contract.Invariant(this.TokenXml != null);
			Contract.Invariant((this.Decryptor != null) == this.IsEncrypted);
		}
#endif
	}
}
