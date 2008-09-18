//-----------------------------------------------------------------------
// <copyright file="MessageDictionary.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging.Reflection {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;

	/// <summary>
	/// Wraps an <see cref="IProtocolMessage"/> instance in a dictionary that
	/// provides access to both well-defined message properties and "extra" 
	/// name/value pairs that have no properties associated with them.
	/// </summary>
	internal class MessageDictionary : IDictionary<string, string> {
		private IProtocolMessage message;

		private MessageDescription description;

		internal MessageDictionary(IProtocolMessage message) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			this.message = message;
			this.description = new MessageDescription(message.GetType());
		}

		#region IDictionary<string,string> Members

		public void Add(string key, string value) {
			if (value == null) {
				throw new ArgumentNullException("value");
			}

			MessagePart part;
			if (this.description.Mapping.TryGetValue(key, out part)) {
				if (part.IsNondefaultValueSet(this.message)) {
					throw new ArgumentException(MessagingStrings.KeyAlreadyExists);
				}
				part.SetValue(this.message, value);
			} else {
				this.message.ExtraData.Add(key, value);
			}
		}

		public bool ContainsKey(string key) {
			return this.message.ExtraData.ContainsKey(key) ||
				(this.description.Mapping.ContainsKey(key) && this.description.Mapping[key].GetValue(this.message) != null);
		}

		public ICollection<string> Keys {
			get {
				List<string> keys = new List<string>(this.message.ExtraData.Count + this.description.Mapping.Count);
				foreach (var pair in this.description.Mapping) {
					// Don't include keys with null values, but default values for structs is ok
					if (pair.Value.GetValue(this.message) != null) {
						keys.Add(pair.Key);
					}
				}

				foreach (string key in this.message.ExtraData.Keys) {
					keys.Add(key);
				}

				return keys.AsReadOnly();
			}
		}

		public bool Remove(string key) {
			if (this.message.ExtraData.Remove(key)) {
				return true;
			} else {
				MessagePart part;
				if (this.description.Mapping.TryGetValue(key, out part)) {
					if (part.GetValue(this.message) != null) {
						part.SetValue(this.message, null);
						return true;
					}
				}
				return false;
			}
		}

		public bool TryGetValue(string key, out string value) {
			MessagePart part;
			if (this.description.Mapping.TryGetValue(key, out part)) {
				value = part.GetValue(this.message);
				return true;
			}
			return this.message.ExtraData.TryGetValue(key, out value);
		}

		public ICollection<string> Values {
			get {
				List<string> values = new List<string>(this.message.ExtraData.Count + this.description.Mapping.Count);
				foreach (MessagePart part in this.description.Mapping.Values) {
					if (part.GetValue(this.message) != null) {
						values.Add(part.GetValue(this.message));
					}
				}

				foreach (string value in this.message.ExtraData.Values) {
					Debug.Assert(value != null, "Null values should never be allowed in the extra data dictionary.");
					values.Add(value);
				}

				return values.AsReadOnly();
			}
		}

		public string this[string key] {
			get {
				MessagePart part;
				if (this.description.Mapping.TryGetValue(key, out part)) {
					// Never throw KeyNotFoundException for declared properties.
					return part.GetValue(this.message);
				} else {
					return this.message.ExtraData[key];
				}
			}

			set {
				MessagePart part;
				if (this.description.Mapping.TryGetValue(key, out part)) {
					part.SetValue(this.message, value);
				} else {
					if (value == null) {
						this.message.ExtraData.Remove(key);
					} else {
						this.message.ExtraData[key] = value;
					}
				}
			}
		}

		#endregion

		#region ICollection<KeyValuePair<string,string>> Members

		public void Add(KeyValuePair<string, string> item) {
			this.Add(item.Key, item.Value);
		}

		public void Clear() {
			foreach (string key in this.Keys) {
				this.Remove(key);
			}
		}

		public bool Contains(KeyValuePair<string, string> item) {
			MessagePart part;
			if (this.description.Mapping.TryGetValue(item.Key, out part)) {
				return string.Equals(part.GetValue(this.message), item.Value, StringComparison.Ordinal);
			} else {
				return this.message.ExtraData.Contains(item);
			}
		}

		void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) {
			foreach (var pair in (IDictionary<string, string>)this) {
				array[arrayIndex++] = pair;
			}
		}

		public int Count {
			get { return this.Keys.Count; }
		}

		bool ICollection<KeyValuePair<string, string>>.IsReadOnly {
			get { return false; }
		}

		public bool Remove(KeyValuePair<string, string> item) {
			// We use contains because that checks that the value is equal as well.
			if (((ICollection<KeyValuePair<string, string>>)this).Contains(item)) {
				((IDictionary<string, string>)this).Remove(item.Key);
				return true;
			}
			return false;
		}

		#endregion

		#region IEnumerable<KeyValuePair<string,string>> Members

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
			foreach (string key in Keys) {
				yield return new KeyValuePair<string, string>(key, this[key]);
			}
		}

		#endregion

		#region IEnumerable Members

		IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return ((IEnumerable<KeyValuePair<string, string>>)this).GetEnumerator();
		}

		#endregion
	}
}
