//-----------------------------------------------------------------------
// <copyright file="MessageDescriptionCollection.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using Validation;

	/// <summary>
	/// A cache of <see cref="MessageDescription"/> instances.
	/// </summary>
	internal class MessageDescriptionCollection : IEnumerable<MessageDescription> {
		/// <summary>
		/// A dictionary of reflected message types and the generated reflection information.
		/// </summary>
		private readonly Dictionary<MessageTypeAndVersion, MessageDescription> reflectedMessageTypes = new Dictionary<MessageTypeAndVersion, MessageDescription>();

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageDescriptionCollection"/> class.
		/// </summary>
		internal MessageDescriptionCollection() {
		}

		#region IEnumerable<MessageDescription> Members

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<MessageDescription> GetEnumerator() {
			lock (this.reflectedMessageTypes) {
				// We must clone the collection so that it's thread-safe to the caller as it leaves our lock.
				return this.reflectedMessageTypes.Values.ToList().GetEnumerator();
			}
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		#endregion

		/// <summary>
		/// Gets a <see cref="MessageDescription"/> instance prepared for the
		/// given message type.
		/// </summary>
		/// <param name="messageType">A type that implements <see cref="IMessage"/>.</param>
		/// <param name="messageVersion">The protocol version of the message.</param>
		/// <returns>A <see cref="MessageDescription"/> instance.</returns>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Diagnostics.Contracts.__ContractsRuntime.Assume(System.Boolean,System.String,System.String)", Justification = "No localization required.")]
		[Pure]
		internal MessageDescription Get(Type messageType, Version messageVersion) {
			RequiresEx.NotNullSubtype<IMessage>(messageType, "messageType");
			Requires.NotNull(messageVersion, "messageVersion");

			MessageTypeAndVersion key = new MessageTypeAndVersion(messageType, messageVersion);

			MessageDescription result;
			lock (this.reflectedMessageTypes) {
				this.reflectedMessageTypes.TryGetValue(key, out result);
			}

			if (result == null) {
				// Construct the message outside the lock.
				var newDescription = new MessageDescription(messageType, messageVersion);

				// Then use the lock again to either acquire what someone else has created in the meantime, or 
				// set and use our own result.
				lock (this.reflectedMessageTypes) {
					if (!this.reflectedMessageTypes.TryGetValue(key, out result)) {
						this.reflectedMessageTypes[key] = result = newDescription;
					}
				}
			}

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
			Requires.NotNull(message, "message");
			return this.Get(message.GetType(), message.Version);
		}

		/// <summary>
		/// Gets the dictionary that provides read/write access to a message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>The dictionary.</returns>
		[Pure]
		internal MessageDictionary GetAccessor(IMessage message) {
			Requires.NotNull(message, "message");
			return this.GetAccessor(message, false);
		}

		/// <summary>
		/// Gets the dictionary that provides read/write access to a message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="getOriginalValues">A value indicating whether this message dictionary will retrieve original values instead of normalized ones.</param>
		/// <returns>The dictionary.</returns>
		[Pure]
		internal MessageDictionary GetAccessor(IMessage message, bool getOriginalValues) {
			Requires.NotNull(message, "message");
			return this.Get(message).GetDictionary(message, getOriginalValues);
		}

		/// <summary>
		/// A struct used as the key to bundle message type and version.
		/// </summary>
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
				Requires.NotNull(messageType, "messageType");
				Requires.NotNull(messageVersion, "messageVersion");

				this.type = messageType;
				this.version = messageVersion;
			}

			/// <summary>
			/// Gets the message type.
			/// </summary>
			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Exposes basic identity on the type.")]
			internal Type Type {
				get { return this.type; }
			}

			/// <summary>
			/// Gets the message version.
			/// </summary>
			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Exposes basic identity on the type.")]
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
