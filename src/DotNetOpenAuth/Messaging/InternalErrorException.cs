//-----------------------------------------------------------------------
// <copyright file="InternalErrorException.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// An internal exception to throw if an internal error within the library requires
	/// an abort of the operation.
	/// </summary>
	/// <remarks>
	/// This exception is internal to prevent clients of the library from catching what is
	/// really an unexpected, potentially unrecoverable exception.
	/// </remarks>
	[global::System.Serializable]
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

		/// <summary>
		/// Initializes a new instance of the <see cref="InternalErrorException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info"/> parameter is null.
		/// </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">
		/// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
		/// </exception>
		protected InternalErrorException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
