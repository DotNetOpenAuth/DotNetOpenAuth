//-----------------------------------------------------------------------
// <copyright file="MessagePart.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Net.Security;
	using System.Reflection;
	using System.Xml;
	using System.Globalization;

	internal class MessagePart {
		private static readonly Dictionary<Type, ValueMapping> converters = new Dictionary<Type, ValueMapping>();

		private ValueMapping converter;

		private PropertyInfo property;

		private FieldInfo field;

		private Type memberDeclaredType;

		private object defaultMemberValue;

		static MessagePart() {
			Map<Uri>(uri => uri.AbsoluteUri, str => new Uri(str));
			Map<DateTime>(dt => XmlConvert.ToString(dt, XmlDateTimeSerializationMode.Utc), str => DateTime.Parse(str));
		}

		internal MessagePart(MemberInfo member, MessagePartAttribute attribute) {
			if (member == null) {
				throw new ArgumentNullException("member");
			}

			this.field = member as FieldInfo;
			this.property = member as PropertyInfo;
			if (this.field == null && this.property == null) {
				throw new ArgumentOutOfRangeException("member"); // TODO: add descriptive message
			}

			if (attribute == null) {
				throw new ArgumentNullException("attribute");
			}

			this.Name = attribute.Name ?? member.Name;
			this.Signed = attribute.Signed;
			this.IsRequired = attribute.IsRequired;
			this.memberDeclaredType = (this.field != null) ? this.field.FieldType : this.property.PropertyType;
			this.defaultMemberValue = deriveDefaultValue(this.memberDeclaredType);

			if (!converters.TryGetValue(this.memberDeclaredType, out this.converter)) {
				this.converter = new ValueMapping(
					obj => obj != null ? obj.ToString() : null,
					str => str != null ? Convert.ChangeType(str, memberDeclaredType) : null);
			}

			// Validate a sane combination of settings
			ValidateSettings();
		}

		internal string Name { get; set; }

		internal ProtectionLevel Signed { get; set; }

		internal bool IsRequired { get; set; }

		internal object ToValue(string value) {
			return this.converter.StringToValue(value);
		}

		internal string ToString(object value) {
			return this.converter.ValueToString(value);
		}

		internal void SetValue(IProtocolMessage message, string value) {
			if (this.property != null) {
				this.property.SetValue(message, this.ToValue(value), null);
			} else {
				this.field.SetValue(message, this.ToValue(value));
			}
		}

		internal string GetValue(IProtocolMessage message) {
			return this.ToString(this.GetValueAsObject(message));
		}

		internal bool IsNondefaultValueSet(IProtocolMessage message) {
			if (this.memberDeclaredType.IsValueType) {
				return !GetValueAsObject(message).Equals(this.defaultMemberValue);
			} else {
				return this.defaultMemberValue != GetValueAsObject(message);
			}
		}

		internal bool IsValidValue(IProtocolMessage message) {
			return true;
		}

		private static object deriveDefaultValue(Type type) {
			if (type.IsValueType) {
				return Activator.CreateInstance(type);
			} else {
				return null;
			}
		}

		private object GetValueAsObject(IProtocolMessage message) {
			if (this.property != null) {
				return this.property.GetValue(message, null);
			} else {
				return this.field.GetValue(message);
			}
		}

		private static void Map<T>(Func<T, string> toString, Func<string, T> toValue) {
			converters.Add(
				typeof(T),
				new ValueMapping(
					obj => obj != null ? toString((T)obj) : null,
					str => str != null ? toValue(str) : default(T)));
		}

		private void ValidateSettings() {
			// An optional tag on a non-nullable value type is a contradiction.
			if (!this.IsRequired && IsNonNullableValueType(this.memberDeclaredType)) {
				MemberInfo member = (MemberInfo)this.field ?? this.property;
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
					"Invalid combination: {0} on message type {1} is a non-nullable value type but is marked as optional.",
					member.Name, member.DeclaringType));
			}
		}

		private static bool IsNonNullableValueType(Type type) {
			if (!type.IsValueType) {
				return false;
			}

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
				return false;
			}

			return true;
		}
	}
}
