//-----------------------------------------------------------------------
// <copyright file="AzureADClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web.Script.Serialization;
	using DotNetOpenAuth.OAuth2;

	public class AzureADClient : WebServerClient
	{
		private static readonly AuthorizationServerDescription AzureADDescription = new AuthorizationServerDescription
		{
			TokenEndpoint = new Uri("https://login.windows.net/common/oauth2/token"),
			AuthorizationEndpoint = new Uri("https://login.windows.net/common/oauth2/authorize?resource=00000002-0000-0000-c000-000000000000/graph.windows.net"),
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="AzureADClient"/> class.
		/// </summary>
		public AzureADClient()
			: base(AzureADDescription)
		{
		}

		#region Methods

		/// <summary>
		/// Parses the access token into an AzureAD token.
		/// </summary>
		/// <param name="token">
		/// The token as a string.
		/// </param>
		/// <returns>
		/// The claims as an object and null in case of failure.
		/// </returns>
		public AzureADClaims ParseAccessToken(string token)
		{
			try
			{
				// This is the encoded JWT token split into the 3 parts
				string[] strparts = token.Split('.');

				// Decparts has the header and claims section decoded from JWT
				string jwtHeader, jwtClaims;
				string jwtb64Header, jwtb64Claims, jwtb64Sig;
				byte[] jwtSig;
				if (strparts.Length != 3)
				{
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

				return claimsAD;
			}
			catch (Exception)
			{
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
		private static string Base64URLdecode(string str)
		{
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
		private static byte[] Base64URLdecodebyte(string str)
		{
			// First replace chars and then pad per spec
			str = str.Replace('-', '+').Replace('_', '/');
			str = str.PadRight(str.Length + ((4 - (str.Length % 4)) % 4), '=');
			return Convert.FromBase64String(str);
		}

		#endregion
	}
}
