//-----------------------------------------------------------------------
// <copyright file="StandardMessageFactoryChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using Reflection;

	public abstract class StandardMessageFactoryChannel : Channel {
		private readonly ICollection<Type> messageTypes;
		private readonly ICollection<Version> versions;

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardMessageFactoryChannel"/> class.
		/// </summary>
		/// <param name="bindingElements">The binding elements.</param>
		protected StandardMessageFactoryChannel(ICollection<Type> messageTypes, ICollection<Version> versions, params IChannelBindingElement[] bindingElements)
			: base(new StandardMessageFactory(), bindingElements) {
			Contract.Requires<ArgumentNullException>(messageTypes != null, "messageTypes");
			Contract.Requires<ArgumentNullException>(versions != null, "versions");

			this.messageTypes = messageTypes;
			this.versions = versions;
			this.StandardMessageFactory.AddMessageTypes(GetMessageDescriptions(this.messageTypes, this.versions, this.MessageDescriptions));
		}

		/// <summary>
		/// Gets or sets a tool that can figure out what kind of message is being received
		/// so it can be deserialized.
		/// </summary>
		protected override IMessageFactory MessageFactory {
			get {
				return (StandardMessageFactory)base.MessageFactory;
			}

			set {
				StandardMessageFactory newValue = (StandardMessageFactory)value;
				base.MessageFactory = newValue;
			}
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
		internal override MessageDescriptionCollection MessageDescriptions {
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

		private static IEnumerable<MessageDescription> GetMessageDescriptions(ICollection<Type> messageTypes, ICollection<Version> versions, MessageDescriptionCollection descriptionsCache)
		{
			Contract.Requires<ArgumentNullException>(messageTypes != null, "messageTypes");
			Contract.Requires<ArgumentNullException>(descriptionsCache != null);
			Contract.Ensures(Contract.Result<IEnumerable<MessageDescription>>() != null);

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
