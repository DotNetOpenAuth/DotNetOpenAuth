//-----------------------------------------------------------------------
// <copyright file="IConsumerCertificateProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System.Security.Cryptography.X509Certificates;

	/// <summary>
	/// A provider that hosts can implement to hook up their RSA-SHA1 binding elements
	/// to their list of known Consumers' certificates.
	/// </summary>
	public interface IConsumerCertificateProvider {
		/// <summary>
		/// Gets the certificate that can be used to verify the signature of an incoming
		/// message from a Consumer.
		/// </summary>
		/// <param name="consumerMessage">The incoming message from some Consumer.</param>
		/// <returns>The public key from the Consumer's X.509 Certificate, if one can be found; otherwise <c>null</c>.</returns>
		X509Certificate2 GetCertificate(ITamperResistantOAuthMessage consumerMessage);
	}
}
