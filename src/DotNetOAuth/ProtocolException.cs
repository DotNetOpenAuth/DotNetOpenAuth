//-----------------------------------------------------------------------
// <copyright file="ProtocolException.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;

	/// <summary>
	/// An exception to represent errors in the local or remote implementation of the protocol.
	/// </summary>
	[Serializable]
	public class ProtocolException : Exception {
		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolException"/> class.
		/// </summary>
		public ProtocolException() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolException"/> class.
		/// </summary>
		/// <param name="message">A message describing the specific error the occurred or was detected.</param>
		public ProtocolException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolException"/> class.
		/// </summary>
		/// <param name="message">A message describing the specific error the occurred or was detected.</param>
		/// <param name="inner">The inner exception to include.</param>
		public ProtocolException(string message, Exception inner) : base(message, inner) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> 
		/// that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The System.Runtime.Serialization.StreamingContext 
		/// that contains contextual information about the source or destination.</param>
		protected ProtocolException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
