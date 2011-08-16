namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;

	public static class OAuthResourceServer {
		private static readonly RSAParameters ResourceServerKeyPair = CreateRSAKey();

		internal static RSACryptoServiceProvider CreateRSA() {
			var rsa = new RSACryptoServiceProvider();
			rsa.ImportParameters(ResourceServerKeyPair);
			return rsa;
		}

		/// <summary>
		/// Creates the RSA key used by all the crypto service provider instances we create.
		/// </summary>
		/// <returns>RSA data that includes the private key.</returns>
		private static RSAParameters CreateRSAKey() {
			// As we generate a new random key, we need to set the UseMachineKeyStore flag so that this doesn't
			// crash on IIS. For more information: 
			// http://social.msdn.microsoft.com/Forums/en-US/clr/thread/7ea48fd0-8d6b-43ed-b272-1a0249ae490f?prof=required
			var cspParameters = new CspParameters();
			cspParameters.Flags = CspProviderFlags.UseArchivableKey | CspProviderFlags.UseMachineKeyStore;
			var asymmetricKey = new RSACryptoServiceProvider(cspParameters);
			return asymmetricKey.ExportParameters(true);
		}
	}
}
