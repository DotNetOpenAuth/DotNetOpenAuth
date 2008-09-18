//-----------------------------------------------------------------------
// <copyright file="MessageDictionary.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging.Reflection {
	using System;
	using System.Collections;
	using System.Collections.Generic;

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

		void IDictionary<string, string>.Add(string key, string value) {
			MessagePart part;
			if (this.description.Mapping.TryGetValue(key, out part)) {
				if (part.GetValue(this.message) != null) {
					throw new ArgumentException(MessagingStrings.KeyAlreadyExists);
				}
				part.SetValue(this.message, value);
			} else {
				this.message.ExtraData.Add(key, value);
			}
		}

		bool IDictionary<string, string>.ContainsKey(string key) {
			return this.message.ExtraData.ContainsKey(key) || this.description.Mapping.ContainsKey(key);
		}

		ICollection<string> IDictionary<string, string>.Keys {
			get {
				string[] keys = new string[this.message.ExtraData.Count + this.description.Mapping.Count];
				int i = 0;
				foreach (string key in this.description.Mapping.Keys) {
					keys[i++] = key;
				}

				foreach (string key in this.message.ExtraData.Keys) {
					keys[i++] = key;
				}

				return keys;
			}
		}

		bool IDictionary<string, string>.Remove(string key) {
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

		bool IDictionary<string, string>.TryGetValue(string key, out string value) {
			MessagePart part;
			if (this.description.Mapping.TryGetValue(key, out part)) {
				value = part.GetValue(this.message);
				return true;
			}
			return this.message.ExtraData.TryGetValue(key, out value);
		}

		ICollection<string> IDictionary<string, string>.Values {
			get {
				string[] values = new string[this.message.ExtraData.Count + this.description.Mapping.Count];
				int i = 0;
				foreach (MessagePart part in this.description.Mapping.Values) {
					values[i++] = part.GetValue(this.message);
				}

				foreach (string value in this.message.ExtraData.Values) {
					values[i++] = value;
				}

				return values;
			}
		}

		string IDictionary<string, string>.this[string key] {
			get {
				MessagePart part;
				if (this.description.Mapping.TryGetValue(key, out part)) {
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
					this.message.ExtraData[key] = value;
				}
			}
		}

		#endregion

		#region ICollection<KeyValuePair<string,string>> Members

		void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item) {
			((IDictionary<string, string>)this).Add(item.Key, item.Value);
		}

		void ICollection<KeyValuePair<string, string>>.Clear() {
			foreach (string key in ((IDictionary<string, string>)this).Keys) {
				((IDictionary<string, string>)this).Remove(key);
			}
		}

		bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item) {
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

		int ICollection<KeyValuePair<string, string>>.Count {
			get { return this.description.Mapping.Count + this.message.ExtraData.Count; }
		}

		bool ICollection<KeyValuePair<string, string>>.IsReadOnly {
			get { return false; }
		}

		bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item) {
			// We use contains because that checks that the value is equal as well.
			if (((ICollection<KeyValuePair<string, string>>)this).Contains(item)) {
				((IDictionary<string, string>)this).Remove(item.Key);
				return true;
			}
			return false;
		}

		#endregion

		#region IEnumerable<KeyValuePair<string,string>> Members

		IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() {
			foreach (MessagePart part in this.description.Mapping.Values) {
				yield return new KeyValuePair<string, string>(part.Name, part.GetValue(this.message));
			}

			foreach (var pair in this.message.ExtraData) {
				yield return pair;
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
