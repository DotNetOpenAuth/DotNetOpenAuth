//-----------------------------------------------------------------------
// <copyright file="StandardMessageFactoryChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Reflection;
	using Validation;

	/// <summary>
	/// A channel that uses the standard message factory.
	/// </summary>
	public abstract class StandardMessageFactoryChannel : Channel {
		/// <summary>
		/// The message types receivable by this channel.
		/// </summary>
		private readonly ICollection<Type> messageTypes;

		/// <summary>
		/// The protocol versions supported by this channel.
		/// </summary>
		private readonly ICollection<Version> versions;

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardMessageFactoryChannel" /> class.
		/// </summary>
		/// <param name="messageTypes">The message types that might be encountered.</param>
		/// <param name="versions">All the possible message versions that might be encountered.</param>
		/// <param name="hostFactories">The host factories.</param>
		/// <param name="bindingElements">The binding elements to use in sending and receiving messages.
		/// The order they are provided is used for outgoing messgaes, and reversed for incoming messages.</param>
		protected StandardMessageFactoryChannel(ICollection<Type> messageTypes, ICollection<Version> versions, IHostFactories hostFactories, IChannelBindingElement[] bindingElements = null)
			: base(new StandardMessageFactory(), bindingElements ?? new IChannelBindingElement[0], hostFactories) {
			Requires.NotNull(messageTypes, "messageTypes");
			Requires.NotNull(versions, "versions");

			this.messageTypes = messageTypes;
			this.versions = versions;
			this.StandardMessageFactory.AddMessageTypes(GetMessageDescriptions(this.messageTypes, this.versions, this.MessageDescriptions));
		}

		/// <summary>
		/// Gets or sets a tool that can figure out what kind of message is being received
		/// so it can be deserialized.
		/// </summary>
		internal StandardMessageFactory StandardMessageFactory {
			get { return (Messaging.StandardMessageFactory)this.MessageFactory; }
			set { this.MessageFactory = value; }
		}

		/// <summary>
		/// Gets or sets the message descriptions.
		/// </summary>
		internal sealed override MessageDescriptionCollection MessageDescriptions {
			get {
				return base.MessageDescriptions;
			}

			set {
				base.MessageDescriptions = value;

				// We must reinitialize the message factory so it can use the new message descriptions.
				var factory = new StandardMessageFactory();
				factory.AddMessageTypes(GetMessageDescriptions(this.messageTypes, this.versions, value));
				this.MessageFactory = factory;
			}
		}

		/// <summary>
		/// Gets or sets a tool that can figure out what kind of message is being received
		/// so it can be deserialized.
		/// </summary>
		protected sealed override IMessageFactory MessageFactory {
			get {
				return (StandardMessageFactory)base.MessageFactory;
			}

			set {
				StandardMessageFactory newValue = (StandardMessageFactory)value;
				base.MessageFactory = newValue;
			}
		}

		/// <summary>
		/// Generates all the message descriptions for a given set of message types and versions.
		/// </summary>
		/// <param name="messageTypes">The message types.</param>
		/// <param name="versions">The message versions.</param>
		/// <param name="descriptionsCache">The cache to use when obtaining the message descriptions.</param>
		/// <returns>The generated/retrieved message descriptions.</returns>
		private static IEnumerable<MessageDescription> GetMessageDescriptions(ICollection<Type> messageTypes, ICollection<Version> versions, MessageDescriptionCollection descriptionsCache)
		{
			Requires.NotNull(messageTypes, "messageTypes");
			Requires.NotNull(descriptionsCache, "descriptionsCache");

			// Get all the MessageDescription objects through the standard cache,
			// so that perhaps it will be a quick lookup, or at least it will be
			// stored there for a quick lookup later.
			var messageDescriptions = new List<MessageDescription>(messageTypes.Count * versions.Count);
			messageDescriptions.AddRange(from version in versions
			                             from messageType in messageTypes
			                             select descriptionsCache.Get(messageType, version));

			return messageDescriptions;
		}
	}
}
