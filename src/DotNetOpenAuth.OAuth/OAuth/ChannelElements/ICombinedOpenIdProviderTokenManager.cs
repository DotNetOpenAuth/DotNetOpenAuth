//-----------------------------------------------------------------------
// <copyright file="ICombinedOpenIdProviderTokenManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using DotNetOpenAuth.OpenId;

	/// <summary>
	/// An interface that providers that play a dual role as OpenID Provider
	/// and OAuth Service Provider should implement on their token manager classes.
	/// </summary>
	/// <remarks>
	/// This interface should be implemented by the same class that implements
	/// <see cref="ITokenManager"/> in order to enable the OpenID+OAuth extension.
	/// </remarks>
	public interface ICombinedOpenIdProviderTokenManager : IOpenIdOAuthTokenManager, ITokenManager {
		/// <summary>
		/// Gets the OAuth consumer key for a given OpenID relying party realm.
		/// </summary>
		/// <param name="realm">The relying party's OpenID realm.</param>
		/// <returns>The OAuth consumer key for a given OpenID realm.</returns>
		/// <para>This is a security-critical function.  Since OpenID requests 
		/// and OAuth extensions for those requests can be formulated by ANYONE 
		/// (no signing is required by the relying party), and since the response to 
		/// the authentication will include access the user is granted to the 
		/// relying party who CLAIMS to be from some realm, it is of paramount 
		/// importance that the realm is recognized as belonging to the consumer 
		/// key by the host service provider in order to protect against phishers.</para>
		string GetConsumerKey(Realm realm);
	}
}
