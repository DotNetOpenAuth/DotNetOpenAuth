//-----------------------------------------------------------------------
// <copyright file="TokenDecryptor.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL).
//     See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
// </license>
// <author>This file was subsequently modified by Andrew Arnott.</author>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.InfoCard {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IdentityModel.Selectors;
	using System.IdentityModel.Tokens;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.ServiceModel.Security;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A utility class for decrypting InfoCard tokens.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Decryptor", Justification = "By design")]
	internal class TokenDecryptor {
		/// <summary>
		/// Backing field for the <see cref="Tokens"/> property.
		/// </summary>
		private List<SecurityToken> tokens;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenDecryptor"/> class.
		/// </summary>
		internal TokenDecryptor() {
			this.tokens = new List<SecurityToken>();
			StoreName storeName = StoreName.My;
			StoreLocation storeLocation = StoreLocation.LocalMachine;
			this.AddDecryptionCertificates(storeName, storeLocation);
		}

		/// <summary>
		/// Gets a list of possible decryption certificates, from the store/location set
		/// </summary>
		/// <remarks>
		/// Defaults to localmachine:my (same place SSL certs are)
		/// </remarks>
		internal List<SecurityToken> Tokens {
			get { return this.tokens; }
		}

		/// <summary>
		/// Adds a certificate to the list of certificates to decrypt with.
		/// </summary>
		/// <param name="certificate">The x509 cert to use for decryption</param>
		internal void AddDecryptionCertificate(X509Certificate2 certificate) {
			this.Tokens.Add(new X509SecurityToken(certificate));
		}

		/// <summary>
		/// Adds a certificate to the list of certificates to decrypt with.
		/// </summary>
		/// <param name="storeName">store name of the certificate</param>
		/// <param name="storeLocation">store location</param>
		/// <param name="thumbprint">thumbprint of the cert to use</param>
		internal void AddDecryptionCertificate(StoreName storeName, StoreLocation storeLocation, string thumbprint) {
			this.AddDecryptionCertificates(
				storeName,
				storeLocation,
				store => store.Find(X509FindType.FindByThumbprint, thumbprint, true));
		}

		/// <summary>
		/// Adds a store of certificates to the list of certificates to decrypt with.
		/// </summary>
		/// <param name="storeName">store name of the certificates</param>
		/// <param name="storeLocation">store location</param>
		internal void AddDecryptionCertificates(StoreName storeName, StoreLocation storeLocation) {
			this.AddDecryptionCertificates(storeName, storeLocation, store => store);
		}

		/// <summary>
		/// Decrpyts a security token from an XML EncryptedData 
		/// </summary>
		/// <param name="reader">The encrypted token XML reader.</param>
		/// <returns>A byte array of the contents of the encrypted token</returns>
		internal byte[] DecryptToken(XmlReader reader) {
			Requires.NotNull(reader, "reader");

			byte[] securityTokenData;
			string encryptionAlgorithm;
			SecurityKeyIdentifier keyIdentifier;
			bool isEmptyElement;

			ErrorUtilities.VerifyInternal(reader.IsStartElement(XmlEncryptionStrings.EncryptedData, XmlEncryptionStrings.Namespace), "Expected encrypted token starting XML element was not found.");
			reader.Read(); // get started

			// if it's not an encryption method, something is dreadfully wrong.
			InfoCardErrorUtilities.VerifyInfoCard(reader.IsStartElement(XmlEncryptionStrings.EncryptionMethod, XmlEncryptionStrings.Namespace), InfoCardStrings.EncryptionAlgorithmNotFound);

			// Looks good, let's grab the alg.
			isEmptyElement = reader.IsEmptyElement;
			encryptionAlgorithm = reader.GetAttribute(XmlEncryptionStrings.Algorithm);
			reader.Read();

			if (!isEmptyElement) {
				while (reader.IsStartElement()) {
					reader.Skip();
				}
				reader.ReadEndElement();
			}

			// get the key identifier
			keyIdentifier = WSSecurityTokenSerializer.DefaultInstance.ReadKeyIdentifier(reader);

			// resolve the symmetric key
			SymmetricSecurityKey decryptingKey = (SymmetricSecurityKey)SecurityTokenResolver.CreateDefaultSecurityTokenResolver(this.tokens.AsReadOnly(), false).ResolveSecurityKey(keyIdentifier[0]);
			SymmetricAlgorithm algorithm = decryptingKey.GetSymmetricAlgorithm(encryptionAlgorithm);

			// dig for the security token data itself.
			reader.ReadStartElement(XmlEncryptionStrings.CipherData, XmlEncryptionStrings.Namespace);
			reader.ReadStartElement(XmlEncryptionStrings.CipherValue, XmlEncryptionStrings.Namespace);
			securityTokenData = Convert.FromBase64String(reader.ReadString());
			reader.ReadEndElement(); // CipherValue
			reader.ReadEndElement(); // CipherData
			reader.ReadEndElement(); // EncryptedData

			// decrypto-magic!
			int blockSizeBytes = algorithm.BlockSize / 8;
			byte[] iv = new byte[blockSizeBytes];
			Buffer.BlockCopy(securityTokenData, 0, iv, 0, iv.Length);
			algorithm.Padding = PaddingMode.ISO10126;
			algorithm.Mode = CipherMode.CBC;
			ICryptoTransform decrTransform = algorithm.CreateDecryptor(algorithm.Key, iv);
			byte[] plainText = decrTransform.TransformFinalBlock(securityTokenData, iv.Length, securityTokenData.Length - iv.Length);
			decrTransform.Dispose();

			return plainText;
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Called by code contracts.")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.Tokens != null);
		}
#endif

		/// <summary>
		/// Adds a store of certificates to the list of certificates to decrypt with.
		/// </summary>
		/// <param name="storeName">store name of the certificates</param>
		/// <param name="storeLocation">store location</param>
		/// <param name="filter">A filter to on the certificates to add.</param>
		private void AddDecryptionCertificates(StoreName storeName, StoreLocation storeLocation, Func<X509Certificate2Collection, X509Certificate2Collection> filter) {
			X509Store store = new X509Store(storeName, storeLocation);
			store.Open(OpenFlags.ReadOnly);

			this.tokens.AddRange((from cert in filter(store.Certificates).Cast<X509Certificate2>()
							 where cert.HasPrivateKey
							 select new X509SecurityToken(cert)).Cast<SecurityToken>());

			store.Close();
		}

		/// <summary>
		/// A set of strings used in parsing the XML token.
		/// </summary>
		internal static class XmlEncryptionStrings {
			/// <summary>
			/// The "http://www.w3.org/2001/04/xmlenc#" value.
			/// </summary>
			internal const string Namespace = "http://www.w3.org/2001/04/xmlenc#";

			/// <summary>
			/// The "EncryptionMethod" value.
			/// </summary>
			internal const string EncryptionMethod = "EncryptionMethod";

			/// <summary>
			/// The "CipherValue" value.
			/// </summary>
			internal const string CipherValue = "CipherValue";

			/// <summary>
			/// The "Algorithm" value.
			/// </summary>
			internal const string Algorithm = "Algorithm";

			/// <summary>
			/// The "EncryptedData" value.
			/// </summary>
			internal const string EncryptedData = "EncryptedData";

			/// <summary>
			/// The "CipherData" value.
			/// </summary>
			internal const string CipherData = "CipherData";
		}
	}
}