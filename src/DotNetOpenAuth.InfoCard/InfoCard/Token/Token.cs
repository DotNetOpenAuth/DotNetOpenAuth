//-----------------------------------------------------------------------
// <copyright file="Token.cs" company="Outercurve Foundation, Microsoft Corporation">
//     Copyright (c) Outercurve Foundation, Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.InfoCard {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.IdentityModel.Claims;
	using System.IdentityModel.Policy;
	using System.IdentityModel.Tokens;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml;
	using System.Xml.XPath;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The decrypted token that was submitted as an Information Card.
	/// </summary>
	[ContractVerification(true)]
	public class Token {
		/// <summary>
		/// Backing field for the <see cref="Claims"/> property.
		/// </summary>
		private IDictionary<string, string> claims;

		/// <summary>
		/// Backing field for the <see cref="UniqueId"/> property.
		/// </summary>
		private string uniqueId;

		/// <summary>
		/// Initializes a new instance of the <see cref="Token"/> class.
		/// </summary>
		/// <param name="tokenXml">Xml token, which may be encrypted.</param>
		/// <param name="audience">The audience.  May be <c>null</c> to avoid audience checking.</param>
		/// <param name="decryptor">The decryptor to use to decrypt the token, if necessary..</param>
		/// <exception cref="InformationCardException">Thrown for any problem decoding or decrypting the token.</exception>
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Not a problem for this type."), SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive")]
		private Token(string tokenXml, Uri audience, TokenDecryptor decryptor) {
			Requires.NotNullOrEmpty(tokenXml, "tokenXml");
			Requires.True(decryptor != null || !IsEncrypted(tokenXml), null);
			Contract.Ensures(this.AuthorizationContext != null);

			byte[] decryptedBytes;
			string decryptedString;

			using (StringReader xmlReader = new StringReader(tokenXml)) {
				var readerSettings = MessagingUtilities.CreateUntrustedXmlReaderSettings();
				using (XmlReader tokenReader = XmlReader.Create(xmlReader, readerSettings)) {
					Contract.Assume(tokenReader != null); // BCL contract should say XmlReader.Create result != null
					if (IsEncrypted(tokenReader)) {
						Logger.InfoCard.DebugFormat("Incoming SAML token, before decryption: {0}", tokenXml);
						decryptedBytes = decryptor.DecryptToken(tokenReader);
						decryptedString = Encoding.UTF8.GetString(decryptedBytes);
						Contract.Assume(decryptedString != null); // BCL contracts should be enhanced here
					} else {
						decryptedBytes = Encoding.UTF8.GetBytes(tokenXml);
						decryptedString = tokenXml;
					}
				}
			}

			var stringReader = new StringReader(decryptedString);
			try {
				this.Xml = new XPathDocument(stringReader).CreateNavigator();
			} catch {
				stringReader.Dispose();
				throw;
			}

			Logger.InfoCard.DebugFormat("Incoming SAML token, after any decryption: {0}", this.Xml.InnerXml);
			this.AuthorizationContext = TokenUtility.AuthenticateToken(this.Xml.ReadSubtree(), audience);
		}

		/// <summary>
		/// Gets the AuthorizationContext behind this token.
		/// </summary>
		public AuthorizationContext AuthorizationContext { get; private set; }

		/// <summary>
		/// Gets the the decrypted token XML.
		/// </summary>
		public XPathNavigator Xml { get; private set; }

		/// <summary>
		/// Gets the UniqueID of this token, usable as a stable username that the user
		/// has already verified belongs to him/her.
		/// </summary>
		/// <remarks>
		/// By default, this uses the PPID and the Issuer's Public Key and hashes them 
		/// together to generate a UniqueID.
		/// </remarks>
		public string UniqueId {
			get {
				if (string.IsNullOrEmpty(this.uniqueId)) {
					this.uniqueId = TokenUtility.GetUniqueName(this.AuthorizationContext);
				}

				return this.uniqueId;
			}
		}

		/// <summary>
		/// Gets the hash of the card issuer's public key.
		/// </summary>
		public string IssuerPubKeyHash {
			get { return TokenUtility.GetIssuerPubKeyHash(this.AuthorizationContext); }
		}

		/// <summary>
		/// Gets the Site Specific ID that the user sees in the Identity Selector.
		/// </summary>
		public string SiteSpecificId {
			get {
				Requires.ValidState(this.Claims.ContainsKey(ClaimTypes.PPID) && !string.IsNullOrEmpty(this.Claims[ClaimTypes.PPID]));
				string ppidValue;
				ErrorUtilities.VerifyOperation(this.Claims.TryGetValue(ClaimTypes.PPID, out ppidValue) && ppidValue != null, InfoCardStrings.PpidClaimRequired);
				return TokenUtility.CalculateSiteSpecificID(ppidValue);
			}
		}

		/// <summary>
		/// Gets the claims in all the claimsets as a dictionary of strings.
		/// </summary>
		public IDictionary<string, string> Claims {
			get {
				if (this.claims == null) {
					this.claims = this.GetFlattenedClaims();
				}

				return this.claims;
			}
		}

		/// <summary>
		/// Deserializes an XML document into a token.
		/// </summary>
		/// <param name="tokenXml">The token XML.</param>
		/// <returns>The deserialized token.</returns>
		public static Token Read(string tokenXml) {
			Requires.NotNullOrEmpty(tokenXml, "tokenXml");
			return Read(tokenXml, (Uri)null);
		}

		/// <summary>
		/// Deserializes an XML document into a token.
		/// </summary>
		/// <param name="tokenXml">The token XML.</param>
		/// <param name="audience">The URI that this token must have been crafted to be sent to.  Use <c>null</c> to accept any intended audience.</param>
		/// <returns>The deserialized token.</returns>
		public static Token Read(string tokenXml, Uri audience) {
			Requires.NotNullOrEmpty(tokenXml, "tokenXml");
			return Read(tokenXml, audience, Enumerable.Empty<SecurityToken>());
		}

		/// <summary>
		/// Deserializes an XML document into a token.
		/// </summary>
		/// <param name="tokenXml">The token XML.</param>
		/// <param name="decryptionTokens">Any X.509 certificates that may be used to decrypt the token, if necessary.</param>
		/// <returns>The deserialized token.</returns>
		public static Token Read(string tokenXml, IEnumerable<SecurityToken> decryptionTokens) {
			Requires.NotNullOrEmpty(tokenXml, "tokenXml");
			Requires.NotNull(decryptionTokens, "decryptionTokens");
			return Read(tokenXml, null, decryptionTokens);
		}

		/// <summary>
		/// Deserializes an XML document into a token.
		/// </summary>
		/// <param name="tokenXml">The token XML.</param>
		/// <param name="audience">The URI that this token must have been crafted to be sent to.  Use <c>null</c> to accept any intended audience.</param>
		/// <param name="decryptionTokens">Any X.509 certificates that may be used to decrypt the token, if necessary.</param>
		/// <returns>The deserialized token.</returns>
		public static Token Read(string tokenXml, Uri audience, IEnumerable<SecurityToken> decryptionTokens) {
			Requires.NotNullOrEmpty(tokenXml, "tokenXml");
			Requires.NotNull(decryptionTokens, "decryptionTokens");
			Contract.Ensures(Contract.Result<Token>() != null);

			TokenDecryptor decryptor = null;

			if (IsEncrypted(tokenXml)) {
				decryptor = new TokenDecryptor();
				decryptor.Tokens.AddRange(decryptionTokens);
			}

			return new Token(tokenXml, audience, decryptor);
		}

		/// <summary>
		/// Determines whether the specified token XML is encrypted.
		/// </summary>
		/// <param name="tokenXml">The token XML.</param>
		/// <returns>
		/// 	<c>true</c> if the specified token XML is encrypted; otherwise, <c>false</c>.
		/// </returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive"), Pure]
		internal static bool IsEncrypted(string tokenXml) {
			Requires.NotNull(tokenXml, "tokenXml");

			var stringReader = new StringReader(tokenXml);
			XmlReader tokenReader;
			try {
				var readerSettings = MessagingUtilities.CreateUntrustedXmlReaderSettings();
				tokenReader = XmlReader.Create(stringReader, readerSettings);
			} catch {
				stringReader.Dispose();
				throw;
			}

			try {
				Contract.Assume(tokenReader != null); // CC missing for XmlReader.Create
				return IsEncrypted(tokenReader);
			} catch {
				IDisposable disposableReader = tokenReader;
				disposableReader.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Determines whether the specified token XML is encrypted.
		/// </summary>
		/// <param name="tokenXmlReader">The token XML.</param>
		/// <returns>
		/// 	<c>true</c> if the specified token XML is encrypted; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsEncrypted(XmlReader tokenXmlReader) {
			Requires.NotNull(tokenXmlReader, "tokenXmlReader");
			return tokenXmlReader.IsStartElement(TokenDecryptor.XmlEncryptionStrings.EncryptedData, TokenDecryptor.XmlEncryptionStrings.Namespace);
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Called by code contracts.")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.AuthorizationContext != null);
		}
#endif

		/// <summary>
		/// Flattens the claims into a dictionary
		/// </summary>
		/// <returns>A dictionary of claim type URIs and claim values.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Expensive call.")]
		[Pure]
		private IDictionary<string, string> GetFlattenedClaims() {
			var flattenedClaims = new Dictionary<string, string>();

			foreach (ClaimSet set in this.AuthorizationContext.ClaimSets) {
				foreach (Claim claim in set) {
					if (claim.Right == Rights.PossessProperty) {
						flattenedClaims.Add(claim.ClaimType, TokenUtility.GetResourceValue(claim));
					}
				}
			}

			return flattenedClaims;
		}
	}
}
