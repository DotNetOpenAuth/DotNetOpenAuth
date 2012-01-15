//-----------------------------------------------------------------------
// <copyright file="ReadOnlyDictionary.cs" company="Microsoft Corporation">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents a read-only dictionary.
	/// </summary>
	/// <typeparam name="K">The type of the key.</typeparam>
	/// <typeparam name="V">The type of the value.</typeparam>
	internal class ReadOnlyDictionary<K, V> : IDictionary<K, V> {
		/// <summary>
		/// Contains base dictionary.
		/// </summary>
		private readonly IDictionary<K, V> baseDictionary;

		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyDictionary&lt;K, V&gt;"/> class.
		/// </summary>
		/// <param name="baseDictionary">The base dictionary.</param>
		public ReadOnlyDictionary(IDictionary<K, V> baseDictionary) {
			this.baseDictionary = baseDictionary;
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </returns>
		public ICollection<K> Keys {
			get { return this.baseDictionary.Keys; }
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </returns>
		public ICollection<V> Values {
			get { return this.baseDictionary.Values; }
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		public int Count {
			get { return this.baseDictionary.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
		/// </returns>
		public bool IsReadOnly {
			get { return true; }
		}

		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// <param name="key">The key being read or written.</param>
		/// <returns>
		/// The element with the specified key.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		/// </exception>
		/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
		/// The property is retrieved and <paramref name="key"/> is not found.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
		/// </exception>
		public V this[K key] {
			get { return this.baseDictionary[key]; }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		/// An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
		/// </exception>
		public void Add(K key, V value) {
			throw new NotSupportedException();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		/// </exception>
		public bool ContainsKey(K key) {
			return this.baseDictionary.ContainsKey(key);
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		/// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
		/// </exception>
		public bool Remove(K key) {
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
		/// <returns>
		/// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		/// </exception>
		public bool TryGetValue(K key, out V value) {
			return this.baseDictionary.TryGetValue(key, out value);
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		public void Add(KeyValuePair<K, V> item) {
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		public void Clear() {
			throw new NotSupportedException();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		public bool Contains(KeyValuePair<K, V> item) {
			return this.baseDictionary.Contains(item);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) {
			this.baseDictionary.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		public bool Remove(KeyValuePair<K, V> item) {
			throw new NotSupportedException();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<KeyValuePair<K, V>> GetEnumerator() {
			return this.baseDictionary.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.baseDictionary.GetEnumerator();
		}
	}
}