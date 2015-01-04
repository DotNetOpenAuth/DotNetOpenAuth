//-----------------------------------------------------------------------
// <copyright file="StandardMessageFactory.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging.Reflection;
	using Validation;

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
			Requires.NotNull(messageTypes, "messageTypes");
			Requires.NullOrNotNullElements(messageTypes, "messageTypes");

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
			Requires.NotNull(recipient, "recipient");
			Requires.NotNull(fields, "fields");

			var matches = this.requestMessageTypes.Keys
				.Where(message => message.CheckMessagePartsPassBasicValidation(fields))
				.OrderByDescending(message => CountInCommon(message.Mapping.Keys, fields.Keys))
				.ThenByDescending(message => message.Mapping.Count)
				.CacheGeneratedResults();
			var match = matches.FirstOrDefault();
			if (match != null) {
				if (Logger.Messaging.IsWarnEnabled() && matches.Count() > 1) {
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
			Requires.NotNull(request, "request");
			Requires.NotNull(fields, "fields");

			var matches = (from responseMessageType in this.responseMessageTypes
			               let message = responseMessageType.Key
			               where message.CheckMessagePartsPassBasicValidation(fields)
			               let ctors = this.FindMatchingResponseConstructors(message, request.GetType())
			               where ctors.Any()
			               orderby GetDerivationDistance(ctors.First().GetParameters()[0].ParameterType, request.GetType()),
			                 CountInCommon(message.Mapping.Keys, fields.Keys) descending,
			                 message.Mapping.Count descending
			               select message).CacheGeneratedResults();
			var match = matches.FirstOrDefault();
			if (match != null) {
				if (Logger.Messaging.IsWarnEnabled() && matches.Count() > 1) {
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
			Requires.NotNull(messageDescription, "messageDescription");
			Requires.NotNull(recipient, "recipient");

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
			Requires.NotNull(messageDescription, "messageDescription");
			Requires.NotNull(request, "request");

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

		/// <summary>
		/// Gets the hierarchical distance between a type and a type it derives from or implements.
		/// </summary>
		/// <param name="assignableType">The base type or interface.</param>
		/// <param name="derivedType">The concrete class that implements the <paramref name="assignableType"/>.</param>
		/// <returns>The distance between the two types.  0 if the types are equivalent, 1 if the type immediately derives from or implements the base type, or progressively higher integers.</returns>
		private static int GetDerivationDistance(Type assignableType, Type derivedType) {
			Requires.NotNull(assignableType, "assignableType");
			Requires.NotNull(derivedType, "derivedType");
			Requires.That(assignableType.IsAssignableFrom(derivedType), "assignableType", "Types are not related as required.");

			// If this is the two types are equivalent...
			if (derivedType.IsAssignableFrom(assignableType))
			{
				return 0;
			}

			int steps;
			derivedType = derivedType.BaseType;
			for (steps = 1; assignableType.IsAssignableFrom(derivedType); steps++)
			{
				derivedType = derivedType.BaseType;
			}

			return steps;
		}

		/// <summary>
		/// Counts how many strings are in the intersection of two collections.
		/// </summary>
		/// <param name="collection1">The first collection.</param>
		/// <param name="collection2">The second collection.</param>
		/// <param name="comparison">The string comparison method to use.</param>
		/// <returns>A non-negative integer no greater than the count of elements in the smallest collection.</returns>
		private static int CountInCommon(ICollection<string> collection1, ICollection<string> collection2, StringComparison comparison = StringComparison.Ordinal) {
			Requires.NotNull(collection1, "collection1");
			Requires.NotNull(collection2, "collection2");

			return collection1.Count(value1 => collection2.Any(value2 => string.Equals(value1, value2, comparison)));
		}

		/// <summary>
		/// Finds constructors for response messages that take a given request message type.
		/// </summary>
		/// <param name="messageDescription">The message description.</param>
		/// <param name="requestType">Type of the request message.</param>
		/// <returns>A sequence of matching constructors.</returns>
		private IEnumerable<ConstructorInfo> FindMatchingResponseConstructors(MessageDescription messageDescription, Type requestType) {
			Requires.NotNull(messageDescription, "messageDescription");
			Requires.NotNull(requestType, "requestType");

			return this.responseMessageTypes[messageDescription].Where(pair => pair.Key.IsAssignableFrom(requestType)).Select(pair => pair.Value);
		}
	}
}
