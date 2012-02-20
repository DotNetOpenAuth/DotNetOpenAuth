//-----------------------------------------------------------------------
// <copyright file="IErrorReporting.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An interface that a Provider site may implement in order to better
	/// control error reporting.
	/// </summary>
	public interface IErrorReporting {
		/// <summary>
		/// Gets the message that can be sent in an error response
		/// with information on who the remote party can contact
		/// for help resolving the error.
		/// </summary>
		/// <value>
		/// The contact address may take any form, as it is intended to be displayed to a person.
		/// </value>
		string Contact { get; }

		/// <summary>
		/// Logs the details of an exception for later reference in diagnosing the problem.
		/// </summary>
		/// <param name="exception">The exception that was generated from the error.</param>
		/// <returns>
		/// A unique identifier for this particular error that the remote party can
		/// reference when contacting <see cref="Contact"/> for help with this error.
		/// May be null.
		/// </returns>
		/// <remarks>
		/// The implementation of this method should never throw an unhandled exception
		/// as that would preclude the ability to send the error response to the remote
		/// party.  When this method is not implemented, it should return null rather
		/// than throwing <see cref="NotImplementedException"/>.
		/// </remarks>
		string LogError(ProtocolException exception);
	}
}
