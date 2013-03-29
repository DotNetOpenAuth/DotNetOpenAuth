//-----------------------------------------------------------------------
// <copyright file="InternalErrorException.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// An internal exception to throw if an internal error within the library requires
	/// an abort of the operation.
	/// </summary>
	/// <remarks>
	/// This exception is internal to prevent clients of the library from catching what is
	/// really an unexpected, potentially unrecoverable exception.
	/// </remarks>
	[SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic", Justification = "We want this to be internal so clients cannot catch it.")]
	internal class InternalErrorException : Exception {
		/// <summary>
		/// Initializes a new instance of the <see cref="InternalErrorException"/> class.
		/// </summary>
		public InternalErrorException() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InternalErrorException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public InternalErrorException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InternalErrorException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner exception.</param>
		public InternalErrorException(string message, Exception inner) : base(message, inner) { }
	}
}
