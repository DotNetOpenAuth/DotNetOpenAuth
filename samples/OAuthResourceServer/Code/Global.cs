namespace OAuthResourceServer.Code {
	using System;
	using System.Linq;
	using System.Security.Cryptography;
	using System.ServiceModel;
	using System.Text;
	using System.Web;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// The web application global events and properties.
	/// </summary>
	public class Global : HttpApplication {
#if SAMPLESONLY
		/// <summary>
		/// The FOR SAMPLE ONLY hard-coded public key of the authorization server that is used to verify the signature on access tokens.
		/// </summary>
		/// <remarks>
		/// In a real app, the authorization server public key would likely come from the server's HTTPS certificate,
		/// but in any case would be determined by the authorization server and its policies.
		/// The hard-coded value used here is so this sample works well with the OAuthAuthorizationServer sample,
		/// which has the corresponding sample private key. 
		/// </remarks>
		public static readonly RSAParameters AuthorizationServerSigningPublicKey = new RSAParameters {
			Exponent = new byte[] { 1, 0, 1 },
			Modulus = new byte[] { 210, 95, 53, 12, 203, 114, 150, 23, 23, 88, 4, 200, 47, 219, 73, 54, 146, 253, 126, 121, 105, 91, 118, 217, 182, 167, 140, 6, 67, 112, 97, 183, 66, 112, 245, 103, 136, 222, 205, 28, 196, 45, 6, 223, 192, 76, 56, 180, 90, 120, 144, 19, 31, 193, 37, 129, 186, 214, 36, 53, 204, 53, 108, 133, 112, 17, 133, 244, 3, 12, 230, 29, 243, 51, 79, 253, 10, 111, 185, 23, 74, 230, 99, 94, 78, 49, 209, 39, 95, 213, 248, 212, 22, 4, 222, 145, 77, 190, 136, 230, 134, 70, 228, 241, 194, 216, 163, 234, 52, 1, 64, 181, 139, 128, 90, 255, 214, 60, 168, 233, 254, 110, 31, 102, 58, 67, 201, 33 },
		};
#else
		[Obsolete("You must use a real key for a real app.", true)]
		public static readonly RSAParameters AuthorizationServerSigningPublicKey = new RSAParameters();
#endif

		/// <summary>
		/// An application memory cache of recent log messages.
		/// </summary>
		public static StringBuilder LogMessages = new StringBuilder();

		/// <summary>
		/// The logger for this sample to use.
		/// </summary>
		public static ILog Logger = LogProvider.GetLogger("DotNetOpenAuth.OAuthResourceServer");

#if SAMPLESONLY
		/// <summary>
		/// The FOR SAMPLE ONLY hard-coded private key used to decrypt access tokens intended for this resource server.
		/// </summary>
		/// <remarks>
		/// In a real app, the resource server would likely use its own HTTPS certificate or some other certificate stored securely
		/// on the server rather than hard-coded in compiled code for security and ease of changing the certificate in case it was compromised.
		/// </remarks>
		internal static readonly RSAParameters ResourceServerEncryptionPrivateKey = new RSAParameters {
			Exponent = new byte[] { 1, 0, 1 },
			Modulus = new byte[] { 166, 175, 117, 169, 211, 251, 45, 215, 55, 53, 202, 65, 153, 155, 92, 219, 235, 243, 61, 170, 101, 250, 221, 214, 239, 175, 238, 175, 239, 20, 144, 72, 227, 221, 4, 219, 32, 225, 101, 96, 18, 33, 117, 176, 110, 123, 109, 23, 29, 85, 93, 50, 129, 163, 113, 57, 122, 212, 141, 145, 17, 31, 67, 165, 181, 91, 117, 23, 138, 251, 198, 132, 188, 213, 10, 157, 116, 229, 48, 168, 8, 127, 28, 156, 239, 124, 117, 36, 232, 100, 222, 23, 52, 186, 239, 5, 63, 207, 185, 16, 137, 73, 137, 147, 252, 71, 9, 239, 113, 27, 88, 255, 91, 56, 192, 142, 210, 21, 34, 81, 204, 239, 57, 60, 140, 249, 15, 101 },
			P = new byte[] { 227, 25, 96, 71, 220, 99, 11, 55, 15, 241, 153, 20, 32, 213, 68, 127, 246, 162, 153, 204, 98, 26, 10, 99, 46, 189, 35, 18, 162, 180, 184, 134, 230, 198, 156, 87, 52, 174, 74, 155, 163, 204, 252, 51, 232, 189, 135, 172, 88, 24, 52, 174, 72, 157, 81, 90, 118, 59, 142, 154, 152, 201, 62, 177 },
			Q = new byte[] { 187, 229, 223, 233, 118, 20, 5, 251, 85, 8, 196, 3, 220, 232, 38, 159, 15, 95, 174, 162, 36, 13, 138, 239, 16, 85, 220, 104, 4, 162, 174, 160, 234, 133, 156, 33, 117, 139, 22, 112, 108, 214, 97, 178, 100, 191, 13, 177, 164, 30, 124, 48, 33, 118, 21, 137, 38, 59, 191, 13, 183, 5, 16, 245 },
			DP = new byte[] { 225, 112, 117, 117, 160, 191, 233, 136, 53, 153, 158, 94, 174, 225, 71, 104, 200, 75, 77, 229, 232, 148, 245, 46, 212, 93, 9, 142, 28, 90, 206, 187, 140, 40, 41, 87, 32, 130, 204, 169, 136, 135, 154, 237, 100, 227, 144, 229, 115, 102, 68, 21, 167, 28, 20, 128, 122, 210, 80, 148, 3, 139, 243, 97 },
			DQ = new byte[] { 133, 252, 100, 207, 232, 184, 92, 143, 157, 82, 115, 220, 65, 81, 118, 0, 228, 136, 153, 81, 219, 157, 160, 157, 218, 171, 47, 81, 41, 69, 12, 123, 136, 224, 159, 182, 40, 72, 119, 70, 210, 5, 137, 131, 25, 94, 55, 152, 157, 236, 115, 40, 43, 36, 54, 53, 39, 131, 97, 56, 153, 114, 206, 101 },
			InverseQ = new byte[] { 129, 119, 84, 118, 29, 35, 194, 186, 96, 169, 7, 7, 200, 22, 187, 34, 72, 131, 200, 246, 79, 120, 49, 242, 8, 220, 74, 114, 195, 95, 90, 108, 80, 2, 212, 71, 125, 100, 184, 77, 203, 236, 64, 122, 108, 212, 150, 129, 66, 248, 218, 3, 186, 71, 213, 236, 142, 66, 33, 196, 150, 216, 138, 114 },
			D = new byte[] { 94, 20, 94, 119, 18, 92, 141, 13, 17, 238, 92, 80, 22, 96, 232, 82, 128, 164, 115, 195, 191, 119, 142, 202, 135, 210, 103, 8, 10, 11, 51, 60, 208, 207, 168, 179, 253, 164, 250, 80, 245, 42, 201, 128, 97, 123, 108, 161, 69, 63, 47, 49, 24, 150, 165, 139, 105, 214, 154, 104, 172, 159, 86, 208, 64, 134, 158, 156, 234, 125, 140, 210, 3, 32, 60, 28, 62, 154, 198, 21, 132, 191, 236, 10, 158, 12, 247, 159, 177, 77, 178, 53, 238, 95, 165, 9, 200, 28, 148, 242, 35, 70, 189, 121, 169, 248, 97, 91, 111, 45, 103, 1, 167, 220, 67, 250, 175, 89, 122, 238, 192, 144, 142, 248, 198, 101, 96, 129 },
		};
#else
		[Obsolete("You must use a real key for a real app.", true)]
		internal static readonly RSAParameters ResourceServerEncryptionPrivateKey = new RSAParameters();
#endif

		/// <summary>
		/// Creates the crypto service provider for this resource server that contains the private key used to decrypt an access token.
		/// </summary>
		/// <returns>An RSA crypto service provider.</returns>
		internal static RSACryptoServiceProvider CreateResourceServerEncryptionServiceProvider() {
			var resourceServerEncryptionServiceProvider = new RSACryptoServiceProvider();
			resourceServerEncryptionServiceProvider.ImportParameters(ResourceServerEncryptionPrivateKey);
			return resourceServerEncryptionServiceProvider;
		}

		/// <summary>
		/// Creates the crypto service provider for the authorization server that contains the public key used to verify an access token signature.
		/// </summary>
		/// <returns>An RSA crypto service provider.</returns>
		internal static RSACryptoServiceProvider CreateAuthorizationServerSigningServiceProvider() {
			var authorizationServerSigningServiceProvider = new RSACryptoServiceProvider();
			authorizationServerSigningServiceProvider.ImportParameters(AuthorizationServerSigningPublicKey);
			return authorizationServerSigningServiceProvider;
		}

		private void Application_Start(object sender, EventArgs e) {
			Logger.Info("Sample starting...");
		}

		private void Application_End(object sender, EventArgs e) {
			Logger.Info("Sample shutting down...");
		}

		private void Application_Error(object sender, EventArgs e) {
			Logger.ErrorException("An unhandled exception occurred in ASP.NET processing: " + Server.GetLastError(), Server.GetLastError());
		}

		private void Application_EndRequest(object sender, EventArgs e) {
		}
	}
}