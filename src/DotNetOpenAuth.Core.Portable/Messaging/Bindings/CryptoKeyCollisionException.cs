//-----------------------------------------------------------------------
// <copyright file="CryptoKeyCollisionException.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Thrown by a hosting application or web site when a cryptographic key is created with a
	/// bucket and handle that conflicts with a previously stored and unexpired key.
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Specialized exception has no need of a message parameter.")]
	public class CryptoKeyCollisionException : ArgumentException {
		/// <summary>
		/// Initializes a new instance of the <see cref="CryptoKeyCollisionException"/> class.
		/// </summary>
		public CryptoKeyCollisionException() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CryptoKeyCollisionException"/> class.
		/// </summary>
		/// <param name="inner">The inner exception to include.</param>
		public CryptoKeyCollisionException(Exception inner) : base(null, inner) {
		}
	}
}
