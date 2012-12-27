//-----------------------------------------------------------------------
// <copyright file="TokenUtility.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL).
//     See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
// </license>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.InfoCard {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Diagnostics.CodeAnalysis;
	using System.IdentityModel.Claims;
	using System.IdentityModel.Policy;
	using System.IdentityModel.Selectors;
	using System.IdentityModel.Tokens;
	using System.IO;
	using System.Linq;
	using System.Net.Mail;
	using System.Security.Cryptography;
	using System.Security.Principal;
	using System.ServiceModel.Security;
	using System.Text;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Tools for reading InfoCard tokens.
	/// </summary>
	internal static class TokenUtility {
		/// <summary>
		/// Gets the maximum amount the token can be out of sync with time.
		/// </summary>
		internal static TimeSpan MaximumClockSkew {
			get { return DotNetOpenAuth.Configuration.DotNetOpenAuthSection.Messaging.MaximumClockSkew; }
		}

		/// <summary>
		/// Token Authentication.  Translates the decrypted data into a AuthContext.
		/// </summary>
		/// <param name="reader">The token XML reader.</param>
		/// <param name="audience">The audience that the token must be scoped for.
		/// Use <c>null</c> to indicate any audience is acceptable.</param>
		/// <returns>
		/// The authorization context carried by the token.
		/// </returns>
		internal static AuthorizationContext AuthenticateToken(XmlReader reader, Uri audience) {
			// Extensibility Point:
			// in order to accept different token types, you would need to add additional 
			// code to create an authenticationcontext from the security token. 
			// This code only supports SamlSecurityToken objects.
			SamlSecurityToken token = WSSecurityTokenSerializer.DefaultInstance.ReadToken(reader, null) as SamlSecurityToken;

			if (null == token) {
				throw new InformationCardException("Unable to read security token");
			}

			if (null != token.SecurityKeys && token.SecurityKeys.Count > 0) {
				throw new InformationCardException("Token Security Keys Exist");
			}

			if (audience == null) {
				Logger.InfoCard.Warn("SAML token Audience checking will be skipped.");
			} else {
				if (token.Assertion.Conditions != null &&
					token.Assertion.Conditions.Conditions != null) {
					foreach (SamlCondition condition in token.Assertion.Conditions.Conditions) {
						SamlAudienceRestrictionCondition audienceCondition = condition as SamlAudienceRestrictionCondition;

						if (audienceCondition != null) {
							Logger.InfoCard.DebugFormat("SAML token audience(s): {0}", audienceCondition.Audiences.ToStringDeferred());
							bool match = audienceCondition.Audiences.Contains(audience);

							if (!match && Logger.InfoCard.IsErrorEnabled) {
								Logger.InfoCard.ErrorFormat("Expected SAML token audience of {0} but found {1}.", audience.AbsoluteUri, audienceCondition.Audiences.Select(aud => aud.AbsoluteUri).ToStringDeferred());
							}

							// The token is invalid if any condition is not valid. 
							// An audience restriction condition is valid if any audience 
							// matches the Relying Party.
							InfoCardErrorUtilities.VerifyInfoCard(match, InfoCardStrings.AudienceMismatch);
						}
					}
				}
			}
			var samlAuthenticator = new SamlSecurityTokenAuthenticator(
				new List<SecurityTokenAuthenticator>(
					new SecurityTokenAuthenticator[] {
						new RsaSecurityTokenAuthenticator(),
						new X509SecurityTokenAuthenticator(),
				}),
				MaximumClockSkew);

			if (audience != null) {
				samlAuthenticator.AllowedAudienceUris.Add(audience.AbsoluteUri);
			}

			return AuthorizationContext.CreateDefaultAuthorizationContext(samlAuthenticator.ValidateToken(token));
		}

		/// <summary>
		/// Translates claims to strings
		/// </summary>
		/// <param name="claim">Claim to translate to a string</param>
		/// <returns>The string representation of a claim's value.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive.")]
		internal static string GetResourceValue(Claim claim) {
			string strClaim = claim.Resource as string;
			if (!string.IsNullOrEmpty(strClaim)) {
				return strClaim;
			}

			IdentityReference reference = claim.Resource as IdentityReference;
			if (null != reference) {
				return reference.Value;
			}

			ICspAsymmetricAlgorithm rsa = claim.Resource as ICspAsymmetricAlgorithm;
			if (null != rsa) {
				using (SHA256 sha = SHA256.Create()) {
					return Convert.ToBase64String(sha.ComputeHash(rsa.ExportCspBlob(false)));
				}
			}

			MailAddress mail = claim.Resource as MailAddress;
			if (null != mail) {
				return mail.ToString();
			}

			byte[] bufferValue = claim.Resource as byte[];
			if (null != bufferValue) {
				return Convert.ToBase64String(bufferValue);
			}

			return claim.Resource.ToString();
		}

		/// <summary>
		/// Generates a UniqueID based off the Issuer's key
		/// </summary>
		/// <param name="authzContext">the Authorization Context</param>
		/// <returns>the hash of the internal key of the issuer</returns>
		internal static string GetIssuerPubKeyHash(AuthorizationContext authzContext) {
			foreach (ClaimSet cs in authzContext.ClaimSets) {
				Claim currentIssuerClaim = GetUniqueRsaClaim(cs.Issuer);

				if (currentIssuerClaim != null) {
					RSA rsa = currentIssuerClaim.Resource as RSA;
					if (null == rsa) {
						return null;
					}

					return ComputeCombinedId(rsa, string.Empty);
				}
			}

			return null;
		}

		/// <summary>
		/// Generates a UniqueID based off the Issuer's key and the PPID.
		/// </summary>
		/// <param name="authzContext">The Authorization Context</param>
		/// <returns>A unique ID for this user at this web site.</returns>
		internal static string GetUniqueName(AuthorizationContext authzContext) {
			Requires.NotNull(authzContext, "authzContext");

			Claim uniqueIssuerClaim = null;
			Claim uniqueUserClaim = null;

			foreach (ClaimSet cs in authzContext.ClaimSets) {
				Claim currentIssuerClaim = GetUniqueRsaClaim(cs.Issuer);

				foreach (Claim c in cs.FindClaims(ClaimTypes.PPID, Rights.PossessProperty)) {
					if (null == currentIssuerClaim) {
						// Found a claim in a ClaimSet with no RSA issuer.
						return null;
					}

					if (null == uniqueUserClaim) {
						uniqueUserClaim = c;
						uniqueIssuerClaim = currentIssuerClaim;
					} else if (!uniqueIssuerClaim.Equals(currentIssuerClaim)) {
						// Found two of the desired claims with different
						// issuers. No unique name.
						return null;
					} else if (!uniqueUserClaim.Equals(c)) {
						// Found two of the desired claims with different
						// values. No unique name.
						return null;
					}
				}
			}

			// No claim of the desired type was found
			if (null == uniqueUserClaim) {
				return null;
			}

			// Unexpected resource type
			string claimValue = uniqueUserClaim.Resource as string;
			if (null == claimValue) {
				return null;
			}

			// Unexpected resource type for RSA
			RSA rsa = uniqueIssuerClaim.Resource as RSA;
			if (null == rsa) {
				return null;
			}

			return ComputeCombinedId(rsa, claimValue);
		}

		/// <summary>
		/// Generates the Site Specific ID to match the one in the Identity Selector.
		/// </summary>
		/// <value>The ID displayed by the Identity Selector.</value>
		/// <param name="ppid">The personal private identifier.</param>
		/// <returns>A string containing the XXX-XXXX-XXX cosmetic value.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive.")]
		internal static string CalculateSiteSpecificID(string ppid) {
			Requires.NotNull(ppid, "ppid");

			int callSignChars = 10;
			char[] charMap = "QL23456789ABCDEFGHJKMNPRSTUVWXYZ".ToCharArray();
			int charMapLength = charMap.Length;

			byte[] raw = Convert.FromBase64String(ppid);
			using (HashAlgorithm hasher = SHA1.Create()) {
				raw = hasher.ComputeHash(raw);
			}

			StringBuilder callSign = new StringBuilder();

			for (int i = 0; i < callSignChars; i++) {
				// after char 3 and char 7, place a dash
				if (i == 3 || i == 7) {
					callSign.Append('-');
				}
				callSign.Append(charMap[raw[i] % charMapLength]);
			}
			return callSign.ToString();
		}

		/// <summary>
		/// Gets the Unique RSA Claim from the SAML token.
		/// </summary>
		/// <param name="cs">the claimset which contains the claim</param>
		/// <returns>a RSA claim</returns>
		private static Claim GetUniqueRsaClaim(ClaimSet cs) {
			Requires.NotNull(cs, "cs");

			Claim rsa = null;

			foreach (Claim c in cs.FindClaims(ClaimTypes.Rsa, Rights.PossessProperty)) {
				if (null == rsa) {
					rsa = c;
				} else if (!rsa.Equals(c)) {
					// Found two non-equal RSA claims
					return null;
				}
			}
			return rsa;
		}

		/// <summary>
		/// Does the actual calculation of a combined ID from a value and an RSA key.
		/// </summary>
		/// <param name="issuerKey">The key of the issuer of the token</param>
		/// <param name="claimValue">the claim value to hash with.</param>
		/// <returns>A base64 representation of the combined ID.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive.")]
		private static string ComputeCombinedId(RSA issuerKey, string claimValue) {
			Requires.NotNull(issuerKey, "issuerKey");
			Requires.NotNull(claimValue, "claimValue");

			int nameLength = Encoding.UTF8.GetByteCount(claimValue);
			RSAParameters rsaParams = issuerKey.ExportParameters(false);
			byte[] shaInput;
			byte[] shaOutput;

			int i = 0;
			shaInput = new byte[rsaParams.Modulus.Length + rsaParams.Exponent.Length + nameLength];
			rsaParams.Modulus.CopyTo(shaInput, i);
			i += rsaParams.Modulus.Length;
			rsaParams.Exponent.CopyTo(shaInput, i);
			i += rsaParams.Exponent.Length;
			i += Encoding.UTF8.GetBytes(claimValue, 0, claimValue.Length, shaInput, i);

			using (SHA256 sha = SHA256.Create()) {
				shaOutput = sha.ComputeHash(shaInput);
			}

			return Convert.ToBase64String(shaOutput);
		}
	}
}
