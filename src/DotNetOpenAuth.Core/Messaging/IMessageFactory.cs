//-----------------------------------------------------------------------
// <copyright file="IMessageFactory.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// A tool to analyze an incoming message to figure out what concrete class
	/// is designed to deserialize it and instantiates that class.
	/// </summary>
	[ContractClass(typeof(IMessageFactoryContract))]
	public interface IMessageFactory {
		/// <summary>
		/// Analyzes an incoming request message payload to discover what kind of 
		/// message is embedded in it and returns the type, or null if no match is found.
		/// </summary>
		/// <param name="recipient">The intended or actual recipient of the request message.</param>
		/// <param name="fields">The name/value pairs that make up the message payload.</param>
		/// <returns>
		/// A newly instantiated <see cref="IProtocolMessage"/>-derived object that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		IDirectedProtocolMessage GetNewRequestMessage(MessageReceivingEndpoint recipient, IDictionary<string, string> fields);

		/// <summary>
		/// Analyzes an incoming request message payload to discover what kind of 
		/// message is embedded in it and returns the type, or null if no match is found.
		/// </summary>
		/// <param name="request">
		/// The message that was sent as a request that resulted in the response.
		/// </param>
		/// <param name="fields">The name/value pairs that make up the message payload.</param>
		/// <returns>
		/// A newly instantiated <see cref="IProtocolMessage"/>-derived object that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		IDirectResponseProtocolMessage GetNewResponseMessage(IDirectedProtocolMessage request, IDictionary<string, string> fields);
	}

	/// <summary>
	/// Code contract for the <see cref="IMessageFactory"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IMessageFactory))]
	internal abstract class IMessageFactoryContract : IMessageFactory {
		/// <summary>
		/// Prevents a default instance of the <see cref="IMessageFactoryContract"/> class from being created.
		/// </summary>
		private IMessageFactoryContract() {
		}

		#region IMessageFactory Members

		/// <summary>
		/// Analyzes an incoming request message payload to discover what kind of
		/// message is embedded in it and returns the type, or null if no match is found.
		/// </summary>
		/// <param name="recipient">The intended or actual recipient of the request message.</param>
		/// <param name="fields">The name/value pairs that make up the message payload.</param>
		/// <returns>
		/// A newly instantiated <see cref="IProtocolMessage"/>-derived object that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		IDirectedProtocolMessage IMessageFactory.GetNewRequestMessage(MessageReceivingEndpoint recipient, IDictionary<string, string> fields) {
			Requires.NotNull(recipient, "recipient");
			Requires.NotNull(fields, "fields");

			throw new NotImplementedException();
		}

		/// <summary>
		/// Analyzes an incoming request message payload to discover what kind of
		/// message is embedded in it and returns the type, or null if no match is found.
		/// </summary>
		/// <param name="request">The message that was sent as a request that resulted in the response.</param>
		/// <param name="fields">The name/value pairs that make up the message payload.</param>
		/// <returns>
		/// A newly instantiated <see cref="IProtocolMessage"/>-derived object that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		IDirectResponseProtocolMessage IMessageFactory.GetNewResponseMessage(IDirectedProtocolMessage request, IDictionary<string, string> fields) {
			Requires.NotNull(request, "request");
			Requires.NotNull(fields, "fields");
			throw new NotImplementedException();
		}

		#endregion
	}
}
