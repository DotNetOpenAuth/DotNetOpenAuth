//-----------------------------------------------------------------------
// <copyright file="IConsumerDescription.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Security.Cryptography.X509Certificates;

	/// <summary>
	/// A description of a consumer from a Service Provider's point of view.
	/// </summary>
	public interface IConsumerDescription {
		/// <summary>
		/// Gets the Consumer key.
		/// </summary>
		string Key { get; }

		/// <summary>
		/// Gets the consumer secret.
		/// </summary>
		string Secret { get; }

		/// <summary>
		/// Gets the certificate that can be used to verify the signature of an incoming
		/// message from a Consumer.
		/// </summary>
		/// <returns>The public key from the Consumer's X.509 Certificate, if one can be found; otherwise <c>null</c>.</returns>
		/// <remarks>
		/// This property must be implemented only if the RSA-SHA1 algorithm is supported by the Service Provider.
		/// </remarks>
		X509Certificate2 Certificate { get; }

		/// <summary>
		/// Gets the callback URI that this consumer has pre-registered with the service provider, if any.
		/// </summary>
		/// <value>A URI that user authorization responses should be directed to; or <c>null</c> if no preregistered callback was arranged.</value>
		Uri Callback { get; }

		/// <summary>
		/// Gets the verification code format that is most appropriate for this consumer
		/// when a callback URI is not available.
		/// </summary>
		/// <value>A set of characters that can be easily keyed in by the user given the Consumer's
		/// application type and form factor.</value>
		/// <remarks>
		/// The value <see cref="OAuth.VerificationCodeFormat.IncludedInCallback"/> should NEVER be returned
		/// since this property is only used in no callback scenarios anyway.
		/// </remarks>
		VerificationCodeFormat VerificationCodeFormat { get; }

		/// <summary>
		/// Gets the length of the verification code to issue for this Consumer.
		/// </summary>
		/// <value>A positive number, generally at least 4.</value>
		int VerificationCodeLength { get; }
	}
}
