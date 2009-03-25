//-----------------------------------------------------------------------
// <copyright file="Token.cs" company="Andrew Arnott, Microsoft Corporation">
//     Copyright (c) Andrew Arnott, Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.InfoCard {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.IdentityModel.Claims;
	using System.IdentityModel.Policy;
	using System.IO;
	using System.Security.Cryptography.X509Certificates;
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
		internal Token(string tokenXml, Uri audience, TokenDecryptor decryptor) {
			Contract.Requires(tokenXml != null && tokenXml.Length > 0);
			Contract.Requires(decryptor != null || !IsEncrypted(tokenXml));
			ErrorUtilities.VerifyNonZeroLength(tokenXml, "tokenXml");

			byte[] decryptedBytes;
			string decryptedString;

			using (XmlReader tokenReader = XmlReader.Create(new StringReader(tokenXml))) {
				if (IsEncrypted(tokenReader)) {
					Logger.InfoCard.DebugFormat("Incoming SAML token, before decryption: {0}", tokenXml);
					ErrorUtilities.VerifyArgumentNotNull(decryptor, "decryptor");
					decryptedBytes = decryptor.DecryptToken(tokenReader);
					decryptedString = Encoding.UTF8.GetString(decryptedBytes);
				} else {
					decryptedBytes = Encoding.UTF8.GetBytes(tokenXml);
					decryptedString = tokenXml;
				}
			}

			this.Xml = new XPathDocument(new StringReader(decryptedString)).CreateNavigator();
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
				Contract.Requires(this.Claims.ContainsKey(ClaimTypes.PPID));
				ErrorUtilities.VerifyOperation(this.Claims.ContainsKey(ClaimTypes.PPID), InfoCardStrings.PpidClaimRequired);
				return TokenUtility.CalculateSiteSpecificID(this.Claims[ClaimTypes.PPID]);
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
		/// Determines whether the specified token XML is encrypted.
		/// </summary>
		/// <param name="tokenXml">The token XML.</param>
		/// <returns>
		/// 	<c>true</c> if the specified token XML is encrypted; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		internal static bool IsEncrypted(string tokenXml) {
			Contract.Requires(tokenXml != null);
			ErrorUtilities.VerifyArgumentNotNull(tokenXml, "tokenXml");

			using (XmlReader tokenReader = XmlReader.Create(new StringReader(tokenXml))) {
				return IsEncrypted(tokenReader);
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
			Contract.Requires(tokenXmlReader != null);
			ErrorUtilities.VerifyArgumentNotNull(tokenXmlReader, "tokenXmlReader");
			return tokenXmlReader.IsStartElement(TokenDecryptor.XmlEncryptionStrings.EncryptedData, TokenDecryptor.XmlEncryptionStrings.Namespace);
		}

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
