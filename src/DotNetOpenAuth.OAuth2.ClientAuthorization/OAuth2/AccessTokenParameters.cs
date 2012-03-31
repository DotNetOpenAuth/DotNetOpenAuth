//-----------------------------------------------------------------------
// <copyright file="AccessTokenParameters.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;

	/// <summary>
	/// Describes the parameters to be fed into creating a response to an access token request.
	/// </summary>
	public class AccessTokenParameters : IDisposable {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenParameters"/> class.
		/// </summary>
		public AccessTokenParameters() {
			this.IncludeRefreshToken = true;
			this.AccessTokenLifetime = TimeSpan.FromHours(1);
		}

		/// <summary>
		/// Gets or sets the access token lifetime.
		/// </summary>
		/// <value>
		/// A positive timespan.
		/// </value>
		/// <remarks>
		/// Note that within this lifetime, authorization <i>may</i> not be revokable.  
		/// Short lifetimes are recommended (e.g. one hour), particularly when the client is not authenticated or
		/// the resources to which access is being granted are sensitive.
		/// </remarks>
		public TimeSpan AccessTokenLifetime { get; set; }

		/// <summary>
		/// Gets the crypto service provider with the asymmetric private key to use for signing access tokens.
		/// </summary>
		/// <returns>A crypto service provider instance that contains the private key.</returns>
		/// <value>Must not be null, and must contain the private key.</value>
		/// <remarks>
		/// The public key in the private/public key pair will be used by the resource
		/// servers to validate that the access token is minted by a trusted authorization server.
		/// </remarks>
		public RSACryptoServiceProvider AccessTokenSigningKey { get; set; }

		/// <summary>
		/// Gets or sets the key to encrypt the access token.
		/// </summary>
		public RSACryptoServiceProvider ResourceServerEncryptionKey { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to provide the client with a refresh token, when applicable.
		/// </summary>
		/// <value>The default value is <c>true</c>.</value>
		/// <remarks>>
		/// The refresh token will never be provided when this value is false.
		/// The refresh token <em>may</em> be provided when this value is true.
		/// </remarks>
		public bool IncludeRefreshToken { get; set; }

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (this.ResourceServerEncryptionKey != null) {
					IDisposable value = this.ResourceServerEncryptionKey;
					value.Dispose();
				}

				if (this.AccessTokenSigningKey != null) {
					IDisposable value = this.AccessTokenSigningKey;
					value.Dispose();
				}
			}
		}

		#endregion
	}
}
