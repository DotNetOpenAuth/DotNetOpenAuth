//-----------------------------------------------------------------------
// <copyright file="HostErrorException.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// An exception to call out a configuration or runtime failure on the part of the
	/// (web) application that is hosting this library.
	/// </summary>
	/// <remarks>
	/// <para>This exception is used rather than <see cref="ProtocolException"/> for those errors
	/// that should never be caught because they indicate a major error in the app itself
	/// or its configuration.</para>
	/// <para>It is an internal exception to assist in making it uncatchable.</para>
	/// </remarks>
	[SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic", Justification = "We don't want this exception to be catchable.")]
	internal class HostErrorException : Exception {
		/// <summary>
		/// Initializes a new instance of the <see cref="HostErrorException"/> class.
		/// </summary>
		internal HostErrorException() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HostErrorException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		internal HostErrorException(string message)
			: base(message) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HostErrorException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner exception.</param>
		internal HostErrorException(string message, Exception inner)
			: base(message, inner) {
		}
	}
}
