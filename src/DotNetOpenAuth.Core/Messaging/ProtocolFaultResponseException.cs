//-----------------------------------------------------------------------
// <copyright file="ProtocolFaultResponseException.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Validation;

	/// <summary>
	/// An exception to represent errors in the local or remote implementation of the protocol
	/// that includes the response message that should be returned to the HTTP client to comply
	/// with the protocol specification.
	/// </summary>
	public class ProtocolFaultResponseException : ProtocolException {
		/// <summary>
		/// The channel that produced the error response message, to be used in constructing the actual HTTP response.
		/// </summary>
		private readonly Channel channel;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolFaultResponseException"/> class
		/// such that it can be sent as a protocol message response to a remote caller.
		/// </summary>
		/// <param name="channel">The channel to use when encoding the response message.</param>
		/// <param name="errorResponse">The message to send back to the HTTP client.</param>
		/// <param name="faultedMessage">The message that was the cause of the exception.  May be null.</param>
		/// <param name="innerException">The inner exception.</param>
		/// <param name="message">The message for the exception.</param>
		protected internal ProtocolFaultResponseException(Channel channel, IDirectResponseProtocolMessage errorResponse, IProtocolMessage faultedMessage = null, Exception innerException = null, string message = null)
			: base(message ?? (innerException != null ? innerException.Message : null), faultedMessage, innerException) {
			Requires.NotNull(channel, "channel");
			Requires.NotNull(errorResponse, "errorResponse");
			this.channel = channel;
			this.ErrorResponseMessage = errorResponse;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolFaultResponseException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> 
		/// that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The System.Runtime.Serialization.StreamingContext 
		/// that contains contextual information about the source or destination.</param>
		protected ProtocolFaultResponseException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the protocol message to send back to the client to report the error.
		/// </summary>
		public IDirectResponseProtocolMessage ErrorResponseMessage { get; private set; }

		/// <summary>
		/// Creates the HTTP response to forward to the client to report the error.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The HTTP response.
		/// </returns>
		public Task<HttpResponseMessage> CreateErrorResponseAsync(CancellationToken cancellationToken) {
			return this.channel.PrepareResponseAsync(this.ErrorResponseMessage, cancellationToken);
		}
	}
}
