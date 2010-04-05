//-----------------------------------------------------------------------
// <copyright file="StandardMessageFactory.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// A message factory that automatically selects the message type based on the incoming data.
	/// </summary>
	internal class StandardMessageFactory : IMessageFactory {
		/// <summary>
		/// The request message types and their constructors to use for instantiating the messages.
		/// </summary>
		private readonly Dictionary<MessageDescription, ConstructorInfo> requestMessageTypes = new Dictionary<MessageDescription, ConstructorInfo>();

		/// <summary>
		/// The response message types and their constructors to use for instantiating the messages.
		/// </summary>
		/// <value>
		/// The value is a dictionary, whose key is the type of the constructor's lone parameter.
		/// </value>
		private readonly Dictionary<MessageDescription, Dictionary<Type, ConstructorInfo>> responseMessageTypes = new Dictionary<MessageDescription, Dictionary<Type, ConstructorInfo>>();

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardMessageFactory"/> class.
		/// </summary>
		internal StandardMessageFactory() {
		}

		/// <summary>
		/// Adds message types to the set that this factory can create.
		/// </summary>
		/// <param name="messageTypes">The message types that this factory may instantiate.</param>
		public virtual void AddMessageTypes(IEnumerable<MessageDescription> messageTypes) {
			Contract.Requires<ArgumentNullException>(messageTypes != null);
			Contract.Requires<ArgumentException>(messageTypes.All(msg => msg != null));

			var unsupportedMessageTypes = new List<MessageDescription>(0);
			foreach (MessageDescription messageDescription in messageTypes) {
				bool supportedMessageType = false;

				// First see whether this message fits the recognized pattern for request messages.
				if (typeof(IDirectedProtocolMessage).IsAssignableFrom(messageDescription.MessageType)) {
					foreach (ConstructorInfo ctor in messageDescription.Constructors) {
						ParameterInfo[] parameters = ctor.GetParameters();
						if (parameters.Length == 2 && parameters[0].ParameterType == typeof(Uri) && parameters[1].ParameterType == typeof(Version)) {
							supportedMessageType = true;
							this.requestMessageTypes.Add(messageDescription, ctor);
							break;
						}
					}
				}

				// Also see if this message fits the recognized pattern for response messages.
				if (typeof(IDirectResponseProtocolMessage).IsAssignableFrom(messageDescription.MessageType)) {
					var responseCtors = new Dictionary<Type, ConstructorInfo>(messageDescription.Constructors.Length);
					foreach (ConstructorInfo ctor in messageDescription.Constructors) {
						ParameterInfo[] parameters = ctor.GetParameters();
						if (parameters.Length == 1 && typeof(IDirectedProtocolMessage).IsAssignableFrom(parameters[0].ParameterType)) {
							responseCtors.Add(parameters[0].ParameterType, ctor);
						}
					}

					if (responseCtors.Count > 0) {
						supportedMessageType = true;
						this.responseMessageTypes.Add(messageDescription, responseCtors);
					}
				}

				if (!supportedMessageType) {
					unsupportedMessageTypes.Add(messageDescription);
				}
			}

			ErrorUtilities.VerifySupported(
				!unsupportedMessageTypes.Any(),
				MessagingStrings.StandardMessageFactoryUnsupportedMessageType,
				unsupportedMessageTypes.ToStringDeferred());
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
		public virtual IDirectedProtocolMessage GetNewRequestMessage(MessageReceivingEndpoint recipient, IDictionary<string, string> fields) {
			MessageDescription matchingType = this.GetMessageDescription(recipient, fields);
			if (matchingType != null) {
				return this.InstantiateAsRequest(matchingType, recipient);
			} else {
				return null;
			}
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
		public virtual IDirectResponseProtocolMessage GetNewResponseMessage(IDirectedProtocolMessage request, IDictionary<string, string> fields) {
			MessageDescription matchingType = this.GetMessageDescription(request, fields);
			if (matchingType != null) {
				return this.InstantiateAsResponse(matchingType, request);
			} else {
				return null;
			}
		}

		#endregion

		/// <summary>
		/// Gets the message type that best fits the given incoming request data.
		/// </summary>
		/// <param name="recipient">The recipient of the incoming data.  Typically not used, but included just in case.</param>
		/// <param name="fields">The data of the incoming message.</param>
		/// <returns>
		/// The message type that matches the incoming data; or <c>null</c> if no match.
		/// </returns>
		/// <exception cref="ProtocolException">May be thrown if the incoming data is ambiguous.</exception>
		protected virtual MessageDescription GetMessageDescription(MessageReceivingEndpoint recipient, IDictionary<string, string> fields) {
			Contract.Requires<ArgumentNullException>(recipient != null);
			Contract.Requires<ArgumentNullException>(fields != null);

			var matches = this.requestMessageTypes.Keys
				.Where(message => message.CheckMessagePartsPassBasicValidation(fields))
				.OrderByDescending(message => message.Mapping.Count)
				.CacheGeneratedResults();
			var match = matches.FirstOrDefault();
			if (match != null) {
				if (Logger.Messaging.IsWarnEnabled && matches.Count() > 1) {
					Logger.Messaging.WarnFormat(
						"Multiple message types seemed to fit the incoming data: {0}",
						matches.ToStringDeferred());
				}

				return match;
			} else {
				// No message type matches the incoming data.
				return null;
			}
		}

		/// <summary>
		/// Gets the message type that best fits the given incoming direct response data.
		/// </summary>
		/// <param name="request">The request message that prompted the response data.</param>
		/// <param name="fields">The data of the incoming message.</param>
		/// <returns>
		/// The message type that matches the incoming data; or <c>null</c> if no match.
		/// </returns>
		/// <exception cref="ProtocolException">May be thrown if the incoming data is ambiguous.</exception>
		protected virtual MessageDescription GetMessageDescription(IDirectedProtocolMessage request, IDictionary<string, string> fields) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Requires<ArgumentNullException>(fields != null);

			var matches = this.responseMessageTypes.Keys
				.Where(message => message.CheckMessagePartsPassBasicValidation(fields))
				.Where(message => this.FindMatchingResponseConstructors(message, request.GetType()).Any())
				.OrderByDescending(message => message.Mapping.Count)
				.CacheGeneratedResults();
			var match = matches.FirstOrDefault();
			if (match != null) {
				if (Logger.Messaging.IsWarnEnabled && matches.Count() > 1) {
					Logger.Messaging.WarnFormat(
						"Multiple message types seemed to fit the incoming data: {0}",
						matches.ToStringDeferred());
				}

				return match;
			} else {
				// No message type matches the incoming data.
				return null;
			}
		}

		/// <summary>
		/// Instantiates the given request message type.
		/// </summary>
		/// <param name="messageDescription">The message description.</param>
		/// <param name="recipient">The recipient.</param>
		/// <returns>The instantiated message.  Never null.</returns>
		protected virtual IDirectedProtocolMessage InstantiateAsRequest(MessageDescription messageDescription, MessageReceivingEndpoint recipient) {
			Contract.Requires<ArgumentNullException>(messageDescription != null);
			Contract.Requires<ArgumentNullException>(recipient != null);
			Contract.Ensures(Contract.Result<IDirectedProtocolMessage>() != null);

			ConstructorInfo ctor = this.requestMessageTypes[messageDescription];
			return (IDirectedProtocolMessage)ctor.Invoke(new object[] { recipient.Location, messageDescription.MessageVersion });
		}

		/// <summary>
		/// Instantiates the given request message type.
		/// </summary>
		/// <param name="messageDescription">The message description.</param>
		/// <param name="request">The request that resulted in this response.</param>
		/// <returns>The instantiated message.  Never null.</returns>
		protected virtual IDirectResponseProtocolMessage InstantiateAsResponse(MessageDescription messageDescription, IDirectedProtocolMessage request) {
			Contract.Requires<ArgumentNullException>(messageDescription != null);
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Ensures(Contract.Result<IDirectResponseProtocolMessage>() != null);

			Type requestType = request.GetType();
			var ctors = this.FindMatchingResponseConstructors(messageDescription, requestType);
			ConstructorInfo ctor = null;
			try {
				ctor = ctors.Single();
			} catch (InvalidOperationException) {
				if (ctors.Any()) {
					ErrorUtilities.ThrowInternal("More than one matching constructor for request type " + requestType.Name + " and response type " + messageDescription.MessageType.Name);
				} else {
					ErrorUtilities.ThrowInternal("Unexpected request message type " + requestType.FullName + " for response type " + messageDescription.MessageType.Name);
				}
			}
			return (IDirectResponseProtocolMessage)ctor.Invoke(new object[] { request });
		}

		private IEnumerable<ConstructorInfo> FindMatchingResponseConstructors(MessageDescription messageDescription, Type requestType) {
			Contract.Requires<ArgumentNullException>(messageDescription != null);
			Contract.Requires<ArgumentNullException>(requestType != null);

			return this.responseMessageTypes[messageDescription].Where(pair => pair.Key.IsAssignableFrom(requestType)).Select(pair => pair.Value);
		}
	}
}
