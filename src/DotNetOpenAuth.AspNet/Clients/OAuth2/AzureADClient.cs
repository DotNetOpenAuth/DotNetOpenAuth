//-----------------------------------------------------------------------
// <copyright file="AzureADClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;
	using System.IdentityModel.Tokens;
	using System.IO;
	using System.Net;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using System.Web;
	using System.Web.Script.Serialization;
	using System.Xml;
	using DotNetOpenAuth.Messaging;

	using Validation;

	/// <summary>
	/// The AzureAD client.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "AzureAD", Justification = "Brand name")]
	public sealed class AzureADClient : OAuth2Client {
		#region Constants and Fields

		/// <summary>
		/// The authorization endpoint.
		/// </summary>
		private const string AuthorizationEndpoint = "https://login.windows.net/common/oauth2/authorize";

		/// <summary>
		/// The token endpoint.
		/// </summary>
		private const string TokenEndpoint = "https://login.windows.net/common/oauth2/token";

		/// <summary>
		/// The name of the graph resource.
		/// </summary>
		private const string GraphResource = "https://graph.windows.net";

		/// <summary>
		/// The URL to get the token decoding certificate from.
		/// </summary>
		private const string MetaDataEndpoint = "https://login.windows.net/evosts.onmicrosoft.com/FederationMetadata/2007-06/FederationMetadata.xml";

		/// <summary>
		/// The URL for AzureAD graph.
		/// </summary>
		private const string GraphEndpoint = "https://graph.windows.net/";

		/// <summary>
		/// The id of the STS.
		/// </summary>
		private const string STSName = "https://sts.windows.net";

		/// <summary>
		/// The app id.
		/// </summary>
		private readonly string appId;

		/// <summary>
		/// The app secret.
		/// </summary>
		private readonly string appSecret;

		/// <summary>
		/// The resource to target.
		/// </summary>
		private readonly string resource;

		/// <summary>
		/// Encoding cert.
		/// </summary>
		private static X509Certificate2[] encodingcert;

		/// <summary>
		/// Hash algo used by the X509Cert.
		/// </summary>
		private static HashAlgorithm hash;

		/// <summary>
		/// The tenantid claim for the authcode.
		/// </summary>
		private string tenantid;

		/// <summary>
		/// The userid claim for the authcode.
		/// </summary>
		private string userid;
		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="AzureADClient"/> class.
		/// </summary>
		/// <param name="appId">
		/// The app id.
		/// </param>
		/// <param name="appSecret">
		/// The app secret.
		/// </param>
		public AzureADClient(string appId, string appSecret)
			: this(appId, appSecret, GraphResource) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AzureADClient"/> class.
		/// </summary>
		/// <param name="appId">
		/// The app id.
		/// </param>
		/// <param name="appSecret">
		/// The app secret.
		/// </param>
		/// <param name="resource">
		/// The resource of oauth request.
		/// </param>
		public AzureADClient(string appId, string appSecret, string resource)
			: base("azuread") {
			Requires.NotNullOrEmpty(appId, "appId");
			Requires.NotNullOrEmpty(appSecret, "appSecret");
			Requires.NotNullOrEmpty(resource, "resource");
			this.appId = appId;
			this.appSecret = appSecret;
			this.resource = resource;
		}
		#endregion

		#region Methods

		/// <summary>
		/// The get service login url.
		/// </summary>
		/// <param name="returnUrl">
		/// The return url.
		/// </param>
		/// <returns>An absolute URI.</returns>
		protected override Uri GetServiceLoginUrl(Uri returnUrl) {
			var builder = new UriBuilder(AuthorizationEndpoint);
			builder.AppendQueryArgs(
				new Dictionary<string, string> {
					{ "client_id", this.appId },
					{ "redirect_uri", returnUrl.AbsoluteUri },
					{ "response_type", "code" },
					{ "resource", this.resource },
				});
				return builder.Uri;
		}

		/// <summary>
		/// The get user data.
		/// </summary>
		/// <param name="accessToken">
		/// The access token.
		/// </param>
		/// <returns>A dictionary of profile data.</returns>
		protected override NameValueCollection GetUserData(string accessToken) {
			var userData = new NameValueCollection();
			try {
				AzureADGraph graphData;
				WebRequest request =
					WebRequest.Create(
						GraphEndpoint + this.tenantid + "/users/" + this.userid + "?api-version=2013-04-05");
				request.Headers = new WebHeaderCollection();
				request.Headers.Add("authorization", accessToken);
				using (var response = request.GetResponse()) {
					using (var responseStream = response.GetResponseStream()) {
						graphData = JsonHelper.Deserialize<AzureADGraph>(responseStream);
					}
				}

				// this dictionary must contains 
				userData.AddItemIfNotEmpty("id", graphData.ObjectId);
				userData.AddItemIfNotEmpty("username", graphData.UserPrincipalName);
				userData.AddItemIfNotEmpty("name", graphData.DisplayName);

				return userData;
			} catch (Exception e) {
				System.Diagnostics.Debug.WriteLine(e.ToStringDescriptive());
				return userData;
			}
		}

		/// <summary>
		/// Obtains an access token given an authorization code and callback URL.
		/// </summary>
		/// <param name="returnUrl">
		/// The return url.
		/// </param>
		/// <param name="authorizationCode">
		/// The authorization code.
		/// </param>
		/// <returns>
		/// The access token.
		/// </returns>
		protected override string QueryAccessToken(Uri returnUrl, string authorizationCode) {
			try {
				var entity =
					MessagingUtilities.CreateQueryString(
						new Dictionary<string, string> {
						{ "client_id", this.appId },
						{ "redirect_uri", returnUrl.AbsoluteUri },
						{ "client_secret", this.appSecret },
						{ "code", authorizationCode },
						{ "grant_type", "authorization_code" },
						{ "api_version", "1.0" },
					});

				WebRequest tokenRequest = WebRequest.Create(TokenEndpoint);
				tokenRequest.ContentType = "application/x-www-form-urlencoded";
				tokenRequest.ContentLength = entity.Length;
				tokenRequest.Method = "POST";

				using (Stream requestStream = tokenRequest.GetRequestStream()) {
					var writer = new StreamWriter(requestStream);
					writer.Write(entity);
					writer.Flush();
				}

				HttpWebResponse tokenResponse = (HttpWebResponse)tokenRequest.GetResponse();
				if (tokenResponse.StatusCode == HttpStatusCode.OK) {
					using (Stream responseStream = tokenResponse.GetResponseStream()) {
						var tokenData = JsonHelper.Deserialize<OAuth2AccessTokenData>(responseStream);
						if (tokenData != null) {
							AzureADClaims claimsAD;
							claimsAD = this.ParseAccessToken(tokenData.AccessToken, true);
							if (claimsAD != null) {
								this.tenantid = claimsAD.Tid;
								this.userid = claimsAD.Oid;
								return tokenData.AccessToken;
							}
							return string.Empty;
						}
					}
				}

				return null;
			} catch (Exception e) {
				System.Diagnostics.Debug.WriteLine(e.ToStringDescriptive());
				return null;
			}
		}

		/// <summary>
		/// Base64 decode function except that it switches -_ to +/ before base64 decode
		/// </summary>
		/// <param name="str">
		/// The string to be base64urldecoded.
		/// </param>
		/// <returns>
		/// Decoded string as string using UTF8 encoding.
		/// </returns>
		private static string Base64URLdecode(string str) {
			System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
			return encoder.GetString(Base64URLdecodebyte(str));
		}

		/// <summary>
		/// Base64 decode function except that it switches -_ to +/ before base64 decode
		/// </summary>
		/// <param name="str">
		/// The string to be base64urldecoded.
		/// </param>
		/// <returns>
		/// Decoded string as bytes.
		/// </returns>
		private static byte[] Base64URLdecodebyte(string str) {
			// First replace chars and then pad per spec
			str = str.Replace('-', '+').Replace('_', '/');
			str = str.PadRight(str.Length + ((4 - (str.Length % 4)) % 4), '=');
			return Convert.FromBase64String(str);
		}

		/// <summary>
		/// Validate whether the unsigned value is same as signed value 
		/// </summary>
		/// <param name="uval">
		/// The raw input of the string signed using the key
		/// </param>
		/// <param name="sval">
		/// The signature of the string
		/// </param>
		/// <param name="certthumb">
		/// The thumbprint of cert used to encrypt token
		/// </param>
		/// <returns>
		/// True if same, false otherwise.
		/// </returns>
		private static bool ValidateSig(byte[] uval, byte[] sval, byte[] certthumb) {
			try {
				bool ret = false;

				X509Certificate2[] certx509 = GetEncodingCert();
				string certthumbhex = string.Empty;

				// Get the hexadecimail representation of the certthumbprint
				for (int i = 0; i < certthumb.Length; i++) {
					certthumbhex += certthumb[i].ToString("X2");
				}

				for (int c = 0; c < certx509.Length; c++) {
					// Skip any cert that does not have the same thumbprint as token
					if (certx509[c].Thumbprint.ToLower() != certthumbhex.ToLower()) {
						continue;
					}
					X509SecurityToken tok = new X509SecurityToken(certx509[c]);
					if (tok == null) {
						return false;
					}
					for (int i = 0; i < tok.SecurityKeys.Count; i++) {
						X509AsymmetricSecurityKey key = tok.SecurityKeys[i] as X509AsymmetricSecurityKey;
						RSACryptoServiceProvider rsa = key.GetAsymmetricAlgorithm(SecurityAlgorithms.RsaSha256Signature, false) as RSACryptoServiceProvider;

						if (rsa == null) {
							continue;
						}
						ret = rsa.VerifyData(uval, hash, sval);
						if (ret == true) {
							return ret;
						}
					}
				}
				return ret;
			} catch (CryptographicException e) {
				Console.WriteLine(e.ToStringDescriptive());
				return false;
			}
		}

		/// <summary>
		/// Returns the certificate with which the token is encoded.
		/// </summary>
		/// <returns>
		/// The encoding certificate.
		/// </returns>
		private static X509Certificate2[] GetEncodingCert() {
			if (encodingcert != null) {
				return encodingcert;
			}
			try {
				// Lock for exclusive access
				lock (typeof(AzureADClient)) {
					XmlDocument doc = new XmlDocument();

					WebRequest request =
					WebRequest.Create(MetaDataEndpoint);
					using (WebResponse response = request.GetResponse()) {
						using (Stream responseStream = response.GetResponseStream()) {
							doc.Load(responseStream);
							XmlNodeList list = doc.GetElementsByTagName("X509Certificate");
							encodingcert = new X509Certificate2[list.Count];
							for (int i = 0; i < list.Count; i++) {
								byte[] todecode_byte = Convert.FromBase64String(list[i].InnerText);
								encodingcert[i] = new X509Certificate2(todecode_byte);
							}
							if (hash == null) {
								hash = SHA256.Create();
							}
						}
					}
				}
				return encodingcert;
			} catch (Exception e) {
				System.Diagnostics.Debug.WriteLine(e.ToStringDescriptive());
				return null;
			}
		}

		/// <summary>
		/// Parses the access token into an AzureAD token.
		/// </summary>
		/// <param name="token">
		/// The token as a string.
		/// </param>
		/// <param name="validate">
		/// Whether to validate against time\audience.
		/// </param>
		/// <returns>
		/// The claims as an object and null in case of failure.
		/// </returns>
		private AzureADClaims ParseAccessToken(string token, bool validate) {
			try {
				// This is the encoded JWT token split into the 3 parts
				string[] strparts = token.Split('.');

				// Decparts has the header and claims section decoded from JWT
				string jwtHeader, jwtClaims;
				string jwtb64Header, jwtb64Claims, jwtb64Sig;
				byte[] jwtSig;
				if (strparts.Length != 3) {
					return null;
				}
				jwtb64Header = strparts[0];
				jwtb64Claims = strparts[1];
				jwtb64Sig = strparts[2];
				jwtHeader = Base64URLdecode(jwtb64Header);
				jwtClaims = Base64URLdecode(jwtb64Claims);
				jwtSig = Base64URLdecodebyte(jwtb64Sig);

				JavaScriptSerializer s1 = new JavaScriptSerializer();

				AzureADClaims claimsAD = s1.Deserialize<AzureADClaims>(jwtClaims);
				AzureADHeader headerAD = s1.Deserialize<AzureADHeader>(jwtHeader);

				if (validate) {
					// Check to see if the token is valid
					// Check if its JWT and RSA encoded
					if (headerAD.Typ.ToUpper() != "JWT") {
						return null;
					}

					// Check if its JWT and RSA encoded
					if (headerAD.Alg.ToUpper() != "RS256") {
						return null;
					}
					if (string.IsNullOrEmpty(headerAD.X5t)) {
						return null;
					}

					// Check audience to be graph
					if (claimsAD.Aud.ToLower().ToLower() != GraphResource.ToLower()) {
						return null;
					}

					// Check issuer to be sts
					if (claimsAD.Iss.ToLower().IndexOf(STSName.ToLower()) != 0) {
						return null;
					}

					// Check time validity
					TimeSpan span = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
					double secsnow = span.TotalSeconds;
					double nbfsecs = Convert.ToDouble(claimsAD.Nbf);
					double expsecs = Convert.ToDouble(claimsAD.Exp);
					if ((nbfsecs - 100 > secsnow) || (secsnow > expsecs + 100)) {
						return null;
					}

					// Validate the signature of the token
					string tokUnsigned = jwtb64Header + "." + jwtb64Claims;
					if (!ValidateSig(Encoding.UTF8.GetBytes(tokUnsigned), jwtSig, Base64URLdecodebyte(headerAD.X5t))) {
						return null;
					}
				}
				return claimsAD;
			} catch (Exception e) {
				System.Diagnostics.Debug.WriteLine(e.ToStringDescriptive());
				return null;
			}
		}
		#endregion
	}
}
