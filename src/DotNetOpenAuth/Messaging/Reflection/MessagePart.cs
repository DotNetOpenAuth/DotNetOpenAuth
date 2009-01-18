//-----------------------------------------------------------------------
// <copyright file="MessagePart.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Net.Security;
	using System.Reflection;
	using System.Xml;
	using DotNetOpenAuth.OpenId;

	/// <summary>
	/// Describes an individual member of a message and assists in its serialization.
	/// </summary>
	internal class MessagePart {
		/// <summary>
		/// A map of converters that help serialize custom objects to string values and back again.
		/// </summary>
		private static readonly Dictionary<Type, ValueMapping> converters = new Dictionary<Type, ValueMapping>();

		/// <summary>
		/// A map of instantiated custom encoders used to encode/decode message parts.
		/// </summary>
		private static readonly Dictionary<Type, IMessagePartEncoder> encoders = new Dictionary<Type, IMessagePartEncoder>();

		/// <summary>
		/// The string-object conversion routines to use for this individual message part.
		/// </summary>
		private ValueMapping converter;

		/// <summary>
		/// The property that this message part is associated with, if aplicable.
		/// </summary>
		private PropertyInfo property;

		/// <summary>
		/// The field that this message part is associated with, if aplicable.
		/// </summary>
		private FieldInfo field;

		/// <summary>
		/// The type of the message part.  (Not the type of the message itself).
		/// </summary>
		private Type memberDeclaredType;

		/// <summary>
		/// The default (uninitialized) value of the member inherent in its type.
		/// </summary>
		private object defaultMemberValue;

		/// <summary>
		/// Initializes static members of the <see cref="MessagePart"/> class.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Much more efficient initialization when we can call methods.")]
		static MessagePart() {
			Map<Uri>(uri => uri.AbsoluteUri, str => new Uri(str));
			Map<DateTime>(dt => XmlConvert.ToString(dt, XmlDateTimeSerializationMode.Utc), str => XmlConvert.ToDateTime(str, XmlDateTimeSerializationMode.Utc));
			Map<byte[]>(bytes => Convert.ToBase64String(bytes), str => Convert.FromBase64String(str));
			Map<Realm>(realm => realm.ToString(), str => new Realm(str));
			Map<Identifier>(id => id.ToString(), str => Identifier.Parse(str));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MessagePart"/> class.
		/// </summary>
		/// <param name="member">
		/// A property or field of an <see cref="IMessage"/> implementing type
		/// that has a <see cref="MessagePartAttribute"/> attached to it.
		/// </param>
		/// <param name="attribute">
		/// The attribute discovered on <paramref name="member"/> that describes the
		/// serialization requirements of the message part.
		/// </param>
		internal MessagePart(MemberInfo member, MessagePartAttribute attribute) {
			if (member == null) {
				throw new ArgumentNullException("member");
			}

			this.field = member as FieldInfo;
			this.property = member as PropertyInfo;
			if (this.field == null && this.property == null) {
				throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						MessagingStrings.UnexpectedType,
						typeof(FieldInfo).Name + ", " + typeof(PropertyInfo).Name,
						member.GetType().Name),
					"member");
			}

			if (attribute == null) {
				throw new ArgumentNullException("attribute");
			}

			this.Name = attribute.Name ?? member.Name;
			this.RequiredProtection = attribute.RequiredProtection;
			this.IsRequired = attribute.IsRequired;
			this.AllowEmpty = attribute.AllowEmpty;
			this.memberDeclaredType = (this.field != null) ? this.field.FieldType : this.property.PropertyType;
			this.defaultMemberValue = DeriveDefaultValue(this.memberDeclaredType);

			if (attribute.Encoder == null) {
				if (!converters.TryGetValue(this.memberDeclaredType, out this.converter)) {
					this.converter = new ValueMapping(
						obj => obj != null ? obj.ToString() : null,
						str => str != null ? Convert.ChangeType(str, this.memberDeclaredType, CultureInfo.InvariantCulture) : null);
				}
			} else {
				var encoder = GetEncoder(attribute.Encoder);
				this.converter = new ValueMapping(
					obj => encoder.Encode(obj),
					str => encoder.Decode(str));
			}

			// readonly and const fields are considered legal, and "constants" for message transport.
			FieldAttributes constAttributes = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault;
			if (this.field != null && (
				(this.field.Attributes & FieldAttributes.InitOnly) == FieldAttributes.InitOnly ||
				(this.field.Attributes & constAttributes) == constAttributes)) {
				this.IsConstantValue = true;
			} else if (this.property != null && !this.property.CanWrite) {
				this.IsConstantValue = true;
			}

			// Validate a sane combination of settings
			this.ValidateSettings();
		}

		/// <summary>
		/// Gets or sets the name to use when serializing or deserializing this parameter in a message.
		/// </summary>
		internal string Name { get; set; }

		/// <summary>
		/// Gets or sets whether this message part must be signed.
		/// </summary>
		internal ProtectionLevel RequiredProtection { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this message part is required for the
		/// containing message to be valid.
		/// </summary>
		internal bool IsRequired { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the string value is allowed to be empty in the serialized message.
		/// </summary>
		internal bool AllowEmpty { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the field or property must remain its default value.
		/// </summary>
		internal bool IsConstantValue { get; set; }

		/// <summary>
		/// Sets the member of a given message to some given value.
		/// Used in deserialization.
		/// </summary>
		/// <param name="message">The message instance containing the member whose value should be set.</param>
		/// <param name="value">The string representation of the value to set.</param>
		internal void SetValue(IMessage message, string value) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			try {
				if (this.IsConstantValue) {
					string constantValue = this.GetValue(message);
					if (!string.Equals(constantValue, value)) {
						throw new ArgumentException(string.Format(
							CultureInfo.CurrentCulture,
							MessagingStrings.UnexpectedMessagePartValueForConstant,
							message.GetType().Name,
							this.Name,
							constantValue,
							value));
					}
				} else {
					if (this.property != null) {
						this.property.SetValue(message, this.ToValue(value), null);
					} else {
						this.field.SetValue(message, this.ToValue(value));
					}
				}
			} catch (FormatException ex) {
				throw ErrorUtilities.Wrap(ex, MessagingStrings.MessagePartReadFailure, message.GetType(), this.Name, value);
			}
		}

		/// <summary>
		/// Gets the value of a member of a given message.
		/// Used in serialization.
		/// </summary>
		/// <param name="message">The message instance to read the value from.</param>
		/// <returns>The string representation of the member's value.</returns>
		internal string GetValue(IMessage message) {
			try {
				object value = this.GetValueAsObject(message);
				return this.ToString(value);
			} catch (FormatException ex) {
				throw ErrorUtilities.Wrap(ex, MessagingStrings.MessagePartWriteFailure, message.GetType(), this.Name);
			}
		}

		/// <summary>
		/// Gets whether the value has been set to something other than its CLR type default value.
		/// </summary>
		/// <param name="message">The message instance to check the value on.</param>
		/// <returns>True if the value is not the CLR default value.</returns>
		internal bool IsNondefaultValueSet(IMessage message) {
			if (this.memberDeclaredType.IsValueType) {
				return !this.GetValueAsObject(message).Equals(this.defaultMemberValue);
			} else {
				return this.defaultMemberValue != this.GetValueAsObject(message);
			}
		}

		/// <summary>
		/// Figures out the CLR default value for a given type.
		/// </summary>
		/// <param name="type">The type whose default value is being sought.</param>
		/// <returns>Either null, or some default value like 0 or 0.0.</returns>
		private static object DeriveDefaultValue(Type type) {
			if (type.IsValueType) {
				return Activator.CreateInstance(type);
			} else {
				return null;
			}
		}

		/// <summary>
		/// Adds a pair of type conversion functions to the static converstion map.
		/// </summary>
		/// <typeparam name="T">The custom type to convert to and from strings.</typeparam>
		/// <param name="toString">The function to convert the custom type to a string.</param>
		/// <param name="toValue">The function to convert a string to the custom type.</param>
		private static void Map<T>(Func<T, string> toString, Func<string, T> toValue) {
			Func<object, string> safeToString = obj => obj != null ? toString((T)obj) : null;
			Func<string, object> safeToT = str => str != null ? toValue(str) : default(T);
			converters.Add(typeof(T), new ValueMapping(safeToString, safeToT));
		}

		/// <summary>
		/// Checks whether a type is a nullable value type (i.e. int?)
		/// </summary>
		/// <param name="type">The type in question.</param>
		/// <returns>True if this is a nullable value type.</returns>
		private static bool IsNonNullableValueType(Type type) {
			if (!type.IsValueType) {
				return false;
			}

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Retrieves a previously instantiated encoder of a given type, or creates a new one and stores it for later retrieval as well.
		/// </summary>
		/// <param name="messagePartEncoder">The message part encoder type.</param>
		/// <returns>An instance of the desired encoder.</returns>
		private static IMessagePartEncoder GetEncoder(Type messagePartEncoder) {
			IMessagePartEncoder encoder;
			if (!encoders.TryGetValue(messagePartEncoder, out encoder)) {
				encoder = encoders[messagePartEncoder] = (IMessagePartEncoder)Activator.CreateInstance(messagePartEncoder);
			}

			return encoder;
		}

		/// <summary>
		/// Converts a string representation of the member's value to the appropriate type.
		/// </summary>
		/// <param name="value">The string representation of the member's value.</param>
		/// <returns>
		/// An instance of the appropriate type for setting the member.
		/// </returns>
		private object ToValue(string value) {
			return value == null ? null : this.converter.StringToValue(value);
		}

		/// <summary>
		/// Converts the member's value to its string representation.
		/// </summary>
		/// <param name="value">The value of the member.</param>
		/// <returns>
		/// The string representation of the member's value.
		/// </returns>
		private string ToString(object value) {
			return value == null ? null : this.converter.ValueToString(value);
		}

		/// <summary>
		/// Gets the value of the message part, without converting it to/from a string.
		/// </summary>
		/// <param name="message">The message instance to read from.</param>
		/// <returns>The value of the member.</returns>
		private object GetValueAsObject(IMessage message) {
			if (this.property != null) {
				return this.property.GetValue(message, null);
			} else {
				return this.field.GetValue(message);
			}
		}

		/// <summary>
		/// Validates that the message part and its attribute have agreeable settings.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Thrown when a non-nullable value type is set as optional.
		/// </exception>
		private void ValidateSettings() {
			if (!this.IsRequired && IsNonNullableValueType(this.memberDeclaredType)) {
				MemberInfo member = (MemberInfo)this.field ?? this.property;
				throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Invalid combination: {0} on message type {1} is a non-nullable value type but is marked as optional.",
						member.Name,
						member.DeclaringType));
			}
		}
	}
}
