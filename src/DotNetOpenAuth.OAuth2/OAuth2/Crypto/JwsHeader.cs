namespace DotNetOpenAuth.OAuth2.Crypto {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class JwsHeader : JwtHeader {
		private JwsHeader() {
		}

		internal JwsHeader(string algorithm) {
			Requires.NotNullOrEmpty(algorithm, "algorithm");
			this.Algorithm = algorithm;
		}

		/// <summary>
		/// Gets or sets a value that identifies the cryptographic algorithm used to secure the JWS.
		/// A list of defined alg values is presented in Section 3, Table 1 of the JSON Web Algorithms (JWA) [JWA]
		/// specification. The processing of the alg header parameter requires that the value MUST be one that is
		/// both supported and for which there exists a key for use with that algorithm associated with the party
		/// that digitally signed or HMACed the content. The alg parameter value is case sensitive.
		/// This header parameter is REQUIRED.
		/// </summary>
		[MessagePart("alg", IsRequired = true, AllowEmpty = false)]
		internal string Algorithm { get; set; }

		/// <summary>
		/// Gets or sets a hint indicating which specific key owned by the signer should be used to validate the digital signature.
		/// This allows signers to explicitly signal a change of key to recipients. The interpretation of the contents of the kid
		/// parameter is unspecified. This header parameter is OPTIONAL.
		/// </summary>
		[MessagePart("kid")]
		internal string KeyIdentity { get; set; }
	}
}
