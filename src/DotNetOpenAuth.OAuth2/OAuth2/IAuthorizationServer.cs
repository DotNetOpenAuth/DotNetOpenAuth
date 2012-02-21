//-----------------------------------------------------------------------
// <copyright file="IAuthorizationServer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// Provides host-specific authorization server services needed by this library.
	/// </summary>
	[ContractClass(typeof(IAuthorizationServerContract))]
	public interface IAuthorizationServer {
		/// <summary>
		/// Gets the store for storing crypto keys used to symmetrically encrypt and sign authorization codes and refresh tokens.
		/// </summary>
		/// <remarks>
		/// This store should be kept strictly confidential in the authorization server(s)
		/// and NOT shared with the resource server.  Anyone with these secrets can mint
		/// tokens to essentially grant themselves access to anything they want.
		/// </remarks>
		ICryptoKeyStore CryptoKeyStore { get; }

		/// <summary>
		/// Gets the authorization code nonce store to use to ensure that authorization codes can only be used once.
		/// </summary>
		/// <value>The authorization code nonce store.</value>
		INonceStore VerificationCodeNonceStore { get; }

		/// <summary>
		/// Gets the crypto service provider with the asymmetric private key to use for signing access tokens.
		/// </summary>
		/// <returns>A crypto service provider instance that contains the private key.</returns>
		/// <value>Must not be null, and must contain the private key.</value>
		/// <remarks>
		/// The public key in the private/public key pair will be used by the resource
		/// servers to validate that the access token is minted by a trusted authorization server.
		/// </remarks>
		RSACryptoServiceProvider AccessTokenSigningKey { get; }

		/// <summary>
		/// Obtains the lifetime for a new access token.
		/// </summary>
		/// <param name="accessTokenRequestMessage">
		/// Details regarding the resources that the access token will grant access to, and the identity of the client
		/// that will receive that access.
		/// Based on this information the receiving resource server can be determined and the lifetime of the access
		/// token can be set based on the sensitivity of the resources.
		/// </param>
		/// <returns>
		/// Receives the lifetime for this access token.  Note that within this lifetime, authorization <i>may</i> not be revokable.  
		/// Short lifetimes are recommended (i.e. one hour), particularly when the client is not authenticated or
		/// the resources to which access is being granted are sensitive.
		/// </returns>
		TimeSpan GetAccessTokenLifetime(IAccessTokenRequest accessTokenRequestMessage);

		/// <summary>
		/// Obtains the encryption key for an access token being created.
		/// </summary>
		/// <param name="accessTokenRequestMessage">
		/// Details regarding the resources that the access token will grant access to, and the identity of the client
		/// that will receive that access.
		/// Based on this information the receiving resource server can be determined and the lifetime of the access
		/// token can be set based on the sensitivity of the resources.
		/// </param>
		/// <returns>
		/// The crypto service provider with the asymmetric public key to use for encrypting access tokens for a specific resource server.
		/// The caller is responsible to dispose of this value.
		/// </returns>
		/// <remarks>
		/// The caller is responsible to dispose of the returned value.
		/// </remarks>
		RSACryptoServiceProvider GetResourceServerEncryptionKey(IAccessTokenRequest accessTokenRequestMessage);

		/// <summary>
		/// Gets the client with a given identifier.
		/// </summary>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <returns>The client registration.  Never null.</returns>
		/// <exception cref="ArgumentException">Thrown when no client with the given identifier is registered with this authorization server.</exception>
		IClientDescription GetClient(string clientIdentifier);

		/// <summary>
		/// Determines whether a described authorization is (still) valid.
		/// </summary>
		/// <param name="authorization">The authorization.</param>
		/// <returns>
		/// 	<c>true</c> if the original authorization is still valid; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// <para>When establishing that an authorization is still valid,
		/// it's very important to only match on recorded authorizations that
		/// meet these criteria:</para>
		///  1) The client identifier matches.
		///  2) The user account matches.
		///  3) The scope on the recorded authorization must include all scopes in the given authorization.
		///  4) The date the recorded authorization was issued must be <em>no later</em> that the date the given authorization was issued.
		/// <para>One possible scenario is where the user authorized a client, later revoked authorization,
		/// and even later reinstated authorization.  This subsequent recorded authorization 
		/// would not satisfy requirement #4 in the above list.  This is important because the revocation
		/// the user went through should invalidate all previously issued tokens as a matter of
		/// security in the event the user was revoking access in order to sever authorization on a stolen
		/// account or piece of hardware in which the tokens were stored. </para>
		/// </remarks>
		bool IsAuthorizationValid(IAuthorizationDescription authorization);
	}

	/// <summary>
	/// Code Contract for the <see cref="IAuthorizationServer"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IAuthorizationServer))]
	internal abstract class IAuthorizationServerContract : IAuthorizationServer {
		/// <summary>
		/// Prevents a default instance of the <see cref="IAuthorizationServerContract"/> class from being created.
		/// </summary>
		private IAuthorizationServerContract() {
		}

		/// <summary>
		/// Gets the store for storeing crypto keys used to symmetrically encrypt and sign authorization codes and refresh tokens.
		/// </summary>
		ICryptoKeyStore IAuthorizationServer.CryptoKeyStore {
			get {
				Contract.Ensures(Contract.Result<ICryptoKeyStore>() != null);
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the authorization code nonce store to use to ensure that authorization codes can only be used once.
		/// </summary>
		/// <value>The authorization code nonce store.</value>
		INonceStore IAuthorizationServer.VerificationCodeNonceStore {
			get {
				Contract.Ensures(Contract.Result<INonceStore>() != null);
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the crypto service provider with the asymmetric private key to use for signing access tokens.
		/// </summary>
		/// <value>
		/// Must not be null, and must contain the private key.
		/// </value>
		/// <returns>A crypto service provider instance that contains the private key.</returns>
		RSACryptoServiceProvider IAuthorizationServer.AccessTokenSigningKey {
			get {
				Contract.Ensures(Contract.Result<RSACryptoServiceProvider>() != null);
				Contract.Ensures(!Contract.Result<RSACryptoServiceProvider>().PublicOnly);
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Obtains the lifetime for a new access token.
		/// </summary>
		/// <param name="accessTokenRequestMessage">Details regarding the resources that the access token will grant access to, and the identity of the client
		/// that will receive that access.
		/// Based on this information the receiving resource server can be determined and the lifetime of the access
		/// token can be set based on the sensitivity of the resources.</param>
		/// <returns>
		/// Receives the lifetime for this access token.  Note that within this lifetime, authorization <i>may</i> not be revokable.
		/// Short lifetimes are recommended (i.e. one hour), particularly when the client is not authenticated or
		/// the resources to which access is being granted are sensitive.
		/// </returns>
		TimeSpan IAuthorizationServer.GetAccessTokenLifetime(IAccessTokenRequest accessTokenRequestMessage) {
			Requires.NotNull(accessTokenRequestMessage, "accessTokenRequestMessage");
			throw new NotImplementedException();
		}

		/// <summary>
		/// Obtains the encryption key for an access token being created.
		/// </summary>
		/// <param name="accessTokenRequestMessage">Details regarding the resources that the access token will grant access to, and the identity of the client
		/// that will receive that access.
		/// Based on this information the receiving resource server can be determined and the lifetime of the access
		/// token can be set based on the sensitivity of the resources.</param>
		/// <returns>
		/// The crypto service provider with the asymmetric public key to use for encrypting access tokens for a specific resource server.
		/// The caller is responsible to dispose of this value.
		/// </returns>
		RSACryptoServiceProvider IAuthorizationServer.GetResourceServerEncryptionKey(IAccessTokenRequest accessTokenRequestMessage) {
			Requires.NotNull(accessTokenRequestMessage, "accessTokenRequestMessage");
			Contract.Ensures(Contract.Result<RSACryptoServiceProvider>() != null);
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the client with a given identifier.
		/// </summary>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <returns>The client registration.  Never null.</returns>
		/// <exception cref="ArgumentException">Thrown when no client with the given identifier is registered with this authorization server.</exception>
		IClientDescription IAuthorizationServer.GetClient(string clientIdentifier) {
			Requires.NotNullOrEmpty(clientIdentifier, "clientIdentifier");
			Contract.Ensures(Contract.Result<IClientDescription>() != null);
			throw new NotImplementedException();
		}

		/// <summary>
		/// Determines whether a described authorization is (still) valid.
		/// </summary>
		/// <param name="authorization">The authorization.</param>
		/// <returns>
		/// 	<c>true</c> if the original authorization is still valid; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// 	<para>When establishing that an authorization is still valid,
		/// it's very important to only match on recorded authorizations that
		/// meet these criteria:</para>
		/// 1) The client identifier matches.
		/// 2) The user account matches.
		/// 3) The scope on the recorded authorization must include all scopes in the given authorization.
		/// 4) The date the recorded authorization was issued must be <em>no later</em> that the date the given authorization was issued.
		/// <para>One possible scenario is where the user authorized a client, later revoked authorization,
		/// and even later reinstated authorization.  This subsequent recorded authorization
		/// would not satisfy requirement #4 in the above list.  This is important because the revocation
		/// the user went through should invalidate all previously issued tokens as a matter of
		/// security in the event the user was revoking access in order to sever authorization on a stolen
		/// account or piece of hardware in which the tokens were stored. </para>
		/// </remarks>
		bool IAuthorizationServer.IsAuthorizationValid(IAuthorizationDescription authorization) {
			Requires.NotNull(authorization, "authorization");
			throw new NotImplementedException();
		}
	}
}
