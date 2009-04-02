//-----------------------------------------------------------------------
// <copyright file="MessageDescriptionCollection.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// A cache of <see cref="MessageDescription"/> instances.
	/// </summary>
	[ContractVerification(true)]
	internal class MessageDescriptionCollection {
		/// <summary>
		/// A dictionary of reflected message types and the generated reflection information.
		/// </summary>
		private readonly Dictionary<MessageTypeAndVersion, MessageDescription> reflectedMessageTypes = new Dictionary<MessageTypeAndVersion, MessageDescription>();

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageDescriptionCollection"/> class.
		/// </summary>
		public MessageDescriptionCollection() {
		}

		/// <summary>
		/// Gets a <see cref="MessageDescription"/> instance prepared for the
		/// given message type.
		/// </summary>
		/// <param name="messageType">A type that implements <see cref="IMessage"/>.</param>
		/// <param name="messageVersion">The protocol version of the message.</param>
		/// <returns>A <see cref="MessageDescription"/> instance.</returns>
		[Pure]
		internal MessageDescription Get(Type messageType, Version messageVersion) {
			Contract.Requires(messageType != null && typeof(IMessage).IsAssignableFrom(messageType));
			Contract.Requires(messageVersion != null);
			Contract.Ensures(Contract.Result<MessageDescription>() != null);
			ErrorUtilities.VerifyArgumentNotNull(messageType, "messageType");
			ErrorUtilities.VerifyArgumentNotNull(messageVersion, "messageVersion");

			MessageTypeAndVersion key = new MessageTypeAndVersion(messageType, messageVersion);

			MessageDescription result;
			if (!this.reflectedMessageTypes.TryGetValue(key, out result)) {
				lock (this.reflectedMessageTypes) {
					if (!this.reflectedMessageTypes.TryGetValue(key, out result)) {
						this.reflectedMessageTypes[key] = result = new MessageDescription(key.Type, key.Version);
					}
				}
			}

			Contract.Assume(result != null); // The reflectedMessageTypes dictionary should never have null values.
			return result;
		}

		/// <summary>
		/// Gets a <see cref="MessageDescription"/> instance prepared for the
		/// given message type.
		/// </summary>
		/// <param name="message">The message for which a <see cref="MessageDescription"/> should be obtained.</param>
		/// <returns>
		/// A <see cref="MessageDescription"/> instance.
		/// </returns>
		[Pure]
		internal MessageDescription Get(IMessage message) {
			Contract.Requires(message != null);
			Contract.Ensures(Contract.Result<MessageDescription>() != null);
			return this.Get(message.GetType(), message.Version);
		}

		/// <summary>
		/// Gets the dictionary that provides read/write access to a message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>The dictionary.</returns>
		[Pure]
		internal MessageDictionary GetAccessor(IMessage message) {
			Contract.Requires(message != null);
			ErrorUtilities.VerifyArgumentNotNull(message, "message");
			return this.Get(message).GetDictionary(message);
		}

		/// <summary>
		/// A struct used as the key to bundle message type and version.
		/// </summary>
		[ContractVerification(true)]
		private struct MessageTypeAndVersion {
			/// <summary>
			/// Backing store for the <see cref="Type"/> property.
			/// </summary>
			private readonly Type type;

			/// <summary>
			/// Backing store for the <see cref="Version"/> property.
			/// </summary>
			private readonly Version version;

			/// <summary>
			/// Initializes a new instance of the <see cref="MessageTypeAndVersion"/> struct.
			/// </summary>
			/// <param name="messageType">Type of the message.</param>
			/// <param name="messageVersion">The message version.</param>
			internal MessageTypeAndVersion(Type messageType, Version messageVersion) {
				Contract.Requires(messageType != null);
				Contract.Requires(messageVersion != null);
				ErrorUtilities.VerifyArgumentNotNull(messageType, "messageType");
				ErrorUtilities.VerifyArgumentNotNull(messageVersion, "messageVersion");

				this.type = messageType;
				this.version = messageVersion;
			}

			/// <summary>
			/// Gets the message type.
			/// </summary>
			internal Type Type {
				get { return this.type; }
			}

			/// <summary>
			/// Gets the message version.
			/// </summary>
			internal Version Version {
				get { return this.version; }
			}

			/// <summary>
			/// Implements the operator ==.
			/// </summary>
			/// <param name="first">The first object to compare.</param>
			/// <param name="second">The second object to compare.</param>
			/// <returns>The result of the operator.</returns>
			public static bool operator ==(MessageTypeAndVersion first, MessageTypeAndVersion second) {
				// structs cannot be null, so this is safe
				return first.Equals(second);
			}

			/// <summary>
			/// Implements the operator !=.
			/// </summary>
			/// <param name="first">The first object to compare.</param>
			/// <param name="second">The second object to compare.</param>
			/// <returns>The result of the operator.</returns>
			public static bool operator !=(MessageTypeAndVersion first, MessageTypeAndVersion second) {
				// structs cannot be null, so this is safe
				return !first.Equals(second);
			}

			/// <summary>
			/// Indicates whether this instance and a specified object are equal.
			/// </summary>
			/// <param name="obj">Another object to compare to.</param>
			/// <returns>
			/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
			/// </returns>
			public override bool Equals(object obj) {
				if (obj is MessageTypeAndVersion) {
					MessageTypeAndVersion other = (MessageTypeAndVersion)obj;
					return this.type == other.type && this.version == other.version;
				} else {
					return false;
				}
			}

			/// <summary>
			/// Returns the hash code for this instance.
			/// </summary>
			/// <returns>
			/// A 32-bit signed integer that is the hash code for this instance.
			/// </returns>
			public override int GetHashCode() {
				return this.type.GetHashCode();
			}
		}
	}
}
