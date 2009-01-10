//-----------------------------------------------------------------------
// <copyright file="MessageDictionary.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;

	/// <summary>
	/// Wraps an <see cref="IMessage"/> instance in a dictionary that
	/// provides access to both well-defined message properties and "extra" 
	/// name/value pairs that have no properties associated with them.
	/// </summary>
	internal class MessageDictionary : IDictionary<string, string> {
		/// <summary>
		/// The <see cref="IMessage"/> instance manipulated by this dictionary.
		/// </summary>
		private IMessage message;

		/// <summary>
		/// The <see cref="MessageDescription"/> instance that describes the message type.
		/// </summary>
		private MessageDescription description;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageDictionary"/> class.
		/// </summary>
		/// <param name="message">The message instance whose values will be manipulated by this dictionary.</param>
		internal MessageDictionary(IMessage message) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			this.message = message;
			this.description = MessageDescription.Get(message.GetType(), message.Version);
		}

		#region ICollection<KeyValuePair<string,string>> Properties

		/// <summary>
		/// Gets the number of explicitly set values in the message.
		/// </summary>
		public int Count {
			get { return this.Keys.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this message is read only.
		/// </summary>
		bool ICollection<KeyValuePair<string, string>>.IsReadOnly {
			get { return false; }
		}

		#endregion

		#region IDictionary<string,string> Properties

		/// <summary>
		/// Gets all the keys that have values associated with them.
		/// </summary>
		public ICollection<string> Keys {
			get {
				List<string> keys = new List<string>(this.message.ExtraData.Count + this.description.Mapping.Count);
				keys.AddRange(this.DeclaredKeys);
				keys.AddRange(this.AdditionalKeys);
				return keys.AsReadOnly();
			}
		}

		/// <summary>
		/// Gets the set of official OAuth keys that have non-null values associated with them.
		/// </summary>
		public ICollection<string> DeclaredKeys {
			get {
				List<string> keys = new List<string>(this.description.Mapping.Count);
				foreach (var pair in this.description.Mapping) {
					// Don't include keys with null values, but default values for structs is ok
					if (pair.Value.GetValue(this.message) != null) {
						keys.Add(pair.Key);
					}
				}

				return keys.AsReadOnly();
			}
		}

		/// <summary>
		/// Gets the keys that are in the message but not declared as official OAuth properties.
		/// </summary>
		public ICollection<string> AdditionalKeys {
			get { return this.message.ExtraData.Keys; }
		}

		/// <summary>
		/// Gets all the values.
		/// </summary>
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

		/// <summary>
		/// Gets or sets a value for some named value.
		/// </summary>
		/// <param name="key">The serialized form of a name for the value to read or write.</param>
		/// <returns>The named value.</returns>
		/// <remarks>
		/// If the key matches a declared property or field on the message type,
		/// that type member is set.  Otherwise the key/value is stored in a
		/// dictionary for extra (weakly typed) strings.
		/// </remarks>
		/// <exception cref="ArgumentException">Thrown when setting a value that is not allowed for a given <paramref name="key"/>.</exception>
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

		#region IDictionary<string,string> Methods

		/// <summary>
		/// Adds a named value to the message.
		/// </summary>
		/// <param name="key">The serialized form of the name whose value is being set.</param>
		/// <param name="value">The serialized form of the value.</param>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="key"/> already has a set value in this message.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="value"/> is null.
		/// </exception>
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

		/// <summary>
		/// Checks whether some named parameter has a value set in the message.
		/// </summary>
		/// <param name="key">The serialized form of the message part's name.</param>
		/// <returns>True if the parameter by the given name has a set value.  False otherwise.</returns>
		public bool ContainsKey(string key) {
			return this.message.ExtraData.ContainsKey(key) ||
				(this.description.Mapping.ContainsKey(key) && this.description.Mapping[key].GetValue(this.message) != null);
		}

		/// <summary>
		/// Removes a name and value from the message given its name.
		/// </summary>
		/// <param name="key">The serialized form of the name to remove.</param>
		/// <returns>True if a message part by the given name was found and removed.  False otherwise.</returns>
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

		/// <summary>
		/// Gets some named value if the key has a value.
		/// </summary>
		/// <param name="key">The name (in serialized form) of the value being sought.</param>
		/// <param name="value">The variable where the value will be set.</param>
		/// <returns>True if the key was found and <paramref name="value"/> was set.  False otherwise.</returns>
		public bool TryGetValue(string key, out string value) {
			MessagePart part;
			if (this.description.Mapping.TryGetValue(key, out part)) {
				value = part.GetValue(this.message);
				return true;
			}
			return this.message.ExtraData.TryGetValue(key, out value);
		}

		#endregion

		#region ICollection<KeyValuePair<string,string>> Methods

		/// <summary>
		/// Sets a named value in the message.
		/// </summary>
		/// <param name="item">The name-value pair to add.  The name is the serialized form of the key.</param>
		public void Add(KeyValuePair<string, string> item) {
			this.Add(item.Key, item.Value);
		}

		/// <summary>
		/// Removes all values in the message.
		/// </summary>
		public void Clear() {
			foreach (string key in this.Keys) {
				this.Remove(key);
			}
		}

		/// <summary>
		/// Checks whether a named value has been set on the message.
		/// </summary>
		/// <param name="item">The name/value pair.</param>
		/// <returns>True if the key exists and has the given value.  False otherwise.</returns>
		public bool Contains(KeyValuePair<string, string> item) {
			MessagePart part;
			if (this.description.Mapping.TryGetValue(item.Key, out part)) {
				return string.Equals(part.GetValue(this.message), item.Value, StringComparison.Ordinal);
			} else {
				return this.message.ExtraData.Contains(item);
			}
		}

		/// <summary>
		/// Copies all the serializable data from the message to a key/value array.
		/// </summary>
		/// <param name="array">The array to copy to.</param>
		/// <param name="arrayIndex">The index in the <paramref name="array"/> to begin copying to.</param>
		void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) {
			foreach (var pair in (IDictionary<string, string>)this) {
				array[arrayIndex++] = pair;
			}
		}

		/// <summary>
		/// Removes a named value from the message if it exists.
		/// </summary>
		/// <param name="item">The serialized form of the name and value to remove.</param>
		/// <returns>True if the name/value was found and removed.  False otherwise.</returns>
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

		/// <summary>
		/// Gets an enumerator that generates KeyValuePair&lt;string, string&gt; instances
		/// for all the key/value pairs that are set in the message.
		/// </summary>
		/// <returns>The enumerator that can generate the name/value pairs.</returns>
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
			foreach (string key in this.Keys) {
				yield return new KeyValuePair<string, string>(key, this[key]);
			}
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Gets an enumerator that generates KeyValuePair&lt;string, string&gt; instances
		/// for all the key/value pairs that are set in the message.
		/// </summary>
		/// <returns>The enumerator that can generate the name/value pairs.</returns>
		IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return ((IEnumerable<KeyValuePair<string, string>>)this).GetEnumerator();
		}

		#endregion
	}
}
