//-----------------------------------------------------------------------
// <copyright file="IAuthorizationServer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// Provides host-specific authorization server services needed by this library.
	/// </summary>
	[ContractClass(typeof(IAuthorizationServerContract))]
	public interface IAuthorizationServer {
		/// <summary>
		/// Gets the secret used to symmetrically encrypt and sign authorization codes and refresh tokens.
		/// </summary>
		/// <remarks>
		/// This secret should be kept strictly confidential in the authorization server(s)
		/// and NOT shared with the resource server.  Anyone with this secret can mint
		/// tokens to essentially grant themselves access to anything they want.
		/// </remarks>
		byte[] Secret { get; }

		/// <summary>
		/// Creates a new instance of the crypto service provider with the asymmetric private key to use for signing access tokens.
		/// </summary>
		/// <value>Must not be null, and must contain the private key.</value>
		/// <remarks>
		/// The public key in the private/public key pair will be used by the resource
		/// servers to validate that the access token is minted by a trusted authorization server.
		/// The caller is responsible to dispose of the returned instance.
		/// </remarks>
		RSACryptoServiceProvider CreateAccessTokenSigningCryptoServiceProvider();

		/// <summary>
		/// Gets the authorization code nonce store to use to ensure that authorization codes can only be used once.
		/// </summary>
		/// <value>The authorization code nonce store.</value>
		INonceStore VerificationCodeNonceStore { get; }

		/// <summary>
		/// Gets the client with a given identifier.
		/// </summary>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <returns>The client registration.  Never null.</returns>
		/// <exception cref="ArgumentException">Thrown when no client with the given identifier is registered with this authorization server.</exception>
		IConsumerDescription GetClient(string clientIdentifier);

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
		/// Gets the secret used to symmetrically encrypt and sign authorization codes and refresh tokens.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// This secret should be kept strictly confidential in the authorization server(s)
		/// and NOT shared with the resource server.  Anyone with this secret can mint
		/// tokens to essentially grant themselves access to anything they want.
		/// </remarks>
		byte[] IAuthorizationServer.Secret {
			get {
				Contract.Ensures(Contract.Result<byte[]>() != null);
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the crypto service provider with the asymmetric private key to use for signing access tokens.
		/// </summary>
		/// <remarks>
		/// The public key in the private/public key pair will be used by the resource
		/// servers to validate that the access token is minted by a trusted authorization server.
		/// </remarks>
		RSACryptoServiceProvider IAuthorizationServer.CreateAccessTokenSigningCryptoServiceProvider() {
			Contract.Ensures(Contract.Result<RSACryptoServiceProvider>() != null);
			Contract.Ensures(!Contract.Result<RSACryptoServiceProvider>().PublicOnly);
			throw new NotImplementedException();
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
		/// Gets the client with a given identifier.
		/// </summary>
		/// <param name="clientIdentifier">The client identifier.</param>
		/// <returns>The client registration.  Never null.</returns>
		/// <exception cref="ArgumentException">Thrown when no client with the given identifier is registered with this authorization server.</exception>
		IConsumerDescription IAuthorizationServer.GetClient(string clientIdentifier) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(clientIdentifier));
			Contract.Ensures(Contract.Result<IConsumerDescription>() != null);
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
			Contract.Requires<ArgumentNullException>(authorization != null, "authorization");
			throw new NotImplementedException();
		}
	}
}
