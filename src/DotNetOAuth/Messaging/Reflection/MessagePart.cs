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

	internal class MessagePart {
		private static readonly Dictionary<Type, ValueMapping> converters = new Dictionary<Type, ValueMapping>();

		private ValueMapping converter;

		private PropertyInfo property;

		private FieldInfo field;

		static MessagePart() {
			Map<Uri>(uri => uri.AbsoluteUri, str => new Uri(str));
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
			this.IsRequired = !attribute.Optional;

			if (!converters.TryGetValue(member.DeclaringType, out this.converter)) {
				Type memberDeclaredType = (this.field != null) ? this.field.FieldType : this.property.PropertyType;
				this.converter = new ValueMapping(
					obj => obj != null ? obj.ToString() : null,
					str => str != null ? Convert.ChangeType(str, memberDeclaredType) : null);
			}
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
			if (this.property != null) {
				return this.ToString(this.property.GetValue(message, null));
			} else {
				return this.ToString(this.field.GetValue(message));
			}
		}

		private static void Map<T>(Func<T, string> toString, Func<string, T> toValue) where T : class {
			converters.Add(
				typeof(T),
				new ValueMapping(
					obj => obj != null ? toString((T)obj) : null,
					str => str != null ? toValue(str) : null));
		}
	}
}
