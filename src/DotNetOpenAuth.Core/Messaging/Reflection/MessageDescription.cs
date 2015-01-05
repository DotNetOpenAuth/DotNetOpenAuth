//-----------------------------------------------------------------------
// <copyright file="MessageDescription.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;

	using DotNetOpenAuth.Logging;

	using Validation;

	/// <summary>
	/// A mapping between serialized key names and <see cref="MessagePart"/> instances describing
	/// those key/values pairs.
	/// </summary>
	internal class MessageDescription {
		/// <summary>
		/// A mapping between the serialized key names and their 
		/// describing <see cref="MessagePart"/> instances.
		/// </summary>
		private Dictionary<string, MessagePart> mapping;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageDescription"/> class.
		/// </summary>
		/// <param name="messageType">Type of the message.</param>
		/// <param name="messageVersion">The message version.</param>
		internal MessageDescription(Type messageType, Version messageVersion) {
			RequiresEx.NotNullSubtype<IMessage>(messageType, "messageType");
			Requires.NotNull(messageVersion, "messageVersion");

			this.MessageType = messageType;
			this.MessageVersion = messageVersion;
			this.ReflectMessageType();
		}

		/// <summary>
		/// Gets the mapping between the serialized key names and their describing
		/// <see cref="MessagePart"/> instances.
		/// </summary>
		internal IDictionary<string, MessagePart> Mapping {
			get { return this.mapping; }
		}

		/// <summary>
		/// Gets the message version this instance was generated from.
		/// </summary>
		internal Version MessageVersion { get; private set; }

		/// <summary>
		/// Gets the type of message this instance was generated from.
		/// </summary>
		/// <value>The type of the described message.</value>
		internal Type MessageType { get; private set; }

		/// <summary>
		/// Gets the constructors available on the message type.
		/// </summary>
		internal ConstructorInfo[] Constructors { get; private set; }

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString() {
			return this.MessageType.Name + " (" + this.MessageVersion + ")";
		}

		/// <summary>
		/// Gets a dictionary that provides read/write access to a message.
		/// </summary>
		/// <param name="message">The message the dictionary should provide access to.</param>
		/// <returns>The dictionary accessor to the message</returns>
		[Pure]
		internal MessageDictionary GetDictionary(IMessage message) {
			Requires.NotNull(message, "message");
			return this.GetDictionary(message, false);
		}

		/// <summary>
		/// Gets a dictionary that provides read/write access to a message.
		/// </summary>
		/// <param name="message">The message the dictionary should provide access to.</param>
		/// <param name="getOriginalValues">A value indicating whether this message dictionary will retrieve original values instead of normalized ones.</param>
		/// <returns>The dictionary accessor to the message</returns>
		[Pure]
		internal MessageDictionary GetDictionary(IMessage message, bool getOriginalValues) {
			Requires.NotNull(message, "message");
			return new MessageDictionary(message, this, getOriginalValues);
		}

		/// <summary>
		/// Ensures the message parts pass basic validation.
		/// </summary>
		/// <param name="parts">The key/value pairs of the serialized message.</param>
		internal void EnsureMessagePartsPassBasicValidation(IDictionary<string, string> parts) {
			try {
				this.CheckRequiredMessagePartsArePresent(parts.Keys, true);
				this.CheckRequiredProtocolMessagePartsAreNotEmpty(parts, true);
				this.CheckMessagePartsConstantValues(parts, true);
			} catch (ProtocolException) {
				Logger.Messaging.ErrorFormat(
					"Error while performing basic validation of {0} ({3}) with these message parts:{1}{2}",
					this.MessageType.Name,
					Environment.NewLine,
					parts.ToStringDeferred(),
					this.MessageVersion);
				throw;
			}
		}

		/// <summary>
		/// Tests whether all the required message parts pass basic validation for the given data.
		/// </summary>
		/// <param name="parts">The key/value pairs of the serialized message.</param>
		/// <returns>A value indicating whether the provided data fits the message's basic requirements.</returns>
		internal bool CheckMessagePartsPassBasicValidation(IDictionary<string, string> parts) {
			Requires.NotNull(parts, "parts");

			return this.CheckRequiredMessagePartsArePresent(parts.Keys, false) &&
				   this.CheckRequiredProtocolMessagePartsAreNotEmpty(parts, false) &&
				   this.CheckMessagePartsConstantValues(parts, false);
		}

		/// <summary>
		/// Verifies that a given set of keys include all the required parameters
		/// for this message type or throws an exception.
		/// </summary>
		/// <param name="keys">The names of all parameters included in a message.</param>
		/// <param name="throwOnFailure">if set to <c>true</c> an exception is thrown on failure with details.</param>
		/// <returns>A value indicating whether the provided data fits the message's basic requirements.</returns>
		/// <exception cref="ProtocolException">
		/// Thrown when required parts of a message are not in <paramref name="keys"/>
		/// if <paramref name="throwOnFailure"/> is <c>true</c>.
		/// </exception>
		private bool CheckRequiredMessagePartsArePresent(IEnumerable<string> keys, bool throwOnFailure) {
			Requires.NotNull(keys, "keys");

			var missingKeys = (from part in this.Mapping.Values
							   where part.IsRequired && !keys.Contains(part.Name)
							   select part.Name).ToArray();
			if (missingKeys.Length > 0) {
				if (throwOnFailure) {
					ErrorUtilities.ThrowProtocol(
						MessagingStrings.RequiredParametersMissing,
						this.MessageType.FullName,
						string.Join(", ", missingKeys));
				} else {
					Logger.Messaging.DebugFormat(
						MessagingStrings.RequiredParametersMissing,
						this.MessageType.FullName,
						missingKeys.ToStringDeferred());
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Ensures the protocol message parts that must not be empty are in fact not empty.
		/// </summary>
		/// <param name="partValues">A dictionary of key/value pairs that make up the serialized message.</param>
		/// <param name="throwOnFailure">if set to <c>true</c> an exception is thrown on failure with details.</param>
		/// <returns>A value indicating whether the provided data fits the message's basic requirements.</returns>
		/// <exception cref="ProtocolException">
		/// Thrown when required parts of a message are not in <paramref name="partValues"/>
		/// if <paramref name="throwOnFailure"/> is <c>true</c>.
		/// </exception>
		private bool CheckRequiredProtocolMessagePartsAreNotEmpty(IDictionary<string, string> partValues, bool throwOnFailure) {
			Requires.NotNull(partValues, "partValues");

			string value;
			var emptyValuedKeys = (from part in this.Mapping.Values
								   where !part.AllowEmpty && partValues.TryGetValue(part.Name, out value) && value != null && value.Length == 0
								   select part.Name).ToArray();
			if (emptyValuedKeys.Length > 0) {
				if (throwOnFailure) {
					ErrorUtilities.ThrowProtocol(
						MessagingStrings.RequiredNonEmptyParameterWasEmpty,
						this.MessageType.FullName,
						string.Join(", ", emptyValuedKeys));
				} else {
					Logger.Messaging.DebugFormat(
						MessagingStrings.RequiredNonEmptyParameterWasEmpty,
						this.MessageType.FullName,
						emptyValuedKeys.ToStringDeferred());
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks that a bunch of message part values meet the constant value requirements of this message description.
		/// </summary>
		/// <param name="partValues">The part values.</param>
		/// <param name="throwOnFailure">if set to <c>true</c>, this method will throw on failure.</param>
		/// <returns>A value indicating whether all the requirements are met.</returns>
		private bool CheckMessagePartsConstantValues(IDictionary<string, string> partValues, bool throwOnFailure) {
			Requires.NotNull(partValues, "partValues");

			var badConstantValues = (from part in this.Mapping.Values
									 where part.IsConstantValueAvailableStatically
									 where partValues.ContainsKey(part.Name)
									 where !string.Equals(partValues[part.Name], part.StaticConstantValue, StringComparison.Ordinal)
									 select part.Name).ToArray();
			if (badConstantValues.Length > 0) {
				if (throwOnFailure) {
					ErrorUtilities.ThrowProtocol(
						MessagingStrings.RequiredMessagePartConstantIncorrect,
						this.MessageType.FullName,
						string.Join(", ", badConstantValues));
				} else {
					Logger.Messaging.DebugFormat(
						MessagingStrings.RequiredMessagePartConstantIncorrect,
						this.MessageType.FullName,
						badConstantValues.ToStringDeferred());
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Reflects over some <see cref="IMessage"/>-implementing type
		/// and prepares to serialize/deserialize instances of that type.
		/// </summary>
		private void ReflectMessageType() {
			this.mapping = new Dictionary<string, MessagePart>();

			Type currentType = this.MessageType;
			do {
				foreach (MemberInfo member in currentType.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) {
					if (member is PropertyInfo || member is FieldInfo) {
						MessagePartAttribute partAttribute =
							(from a in member.GetCustomAttributes(typeof(MessagePartAttribute), true).OfType<MessagePartAttribute>()
							 orderby a.MinVersionValue descending
							 where a.MinVersionValue <= this.MessageVersion
							 where a.MaxVersionValue >= this.MessageVersion
							 select a).FirstOrDefault();
						if (partAttribute != null) {
							MessagePart part = new MessagePart(member, partAttribute);
							if (this.mapping.ContainsKey(part.Name)) {
								Logger.Messaging.WarnFormat(
									"Message type {0} has more than one message part named {1}.  Inherited members will be hidden.",
									this.MessageType.Name,
									part.Name);
							} else {
								this.mapping.Add(part.Name, part);
							}
						}
					}
				}
				currentType = currentType.BaseType;
			} while (currentType != null);

			BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
			this.Constructors = this.MessageType.GetConstructors(flags);
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Describes traits of this class that are always true.
		/// </summary>
		[ContractInvariantMethod]
		private void Invariant() {
			Contract.Invariant(this.MessageType != null);
			Contract.Invariant(this.MessageVersion != null);
			Contract.Invariant(this.Constructors != null);
		}
#endif
	}
}
