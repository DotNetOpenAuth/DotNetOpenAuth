using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Session {
	/// <summary>
	/// A simple key/value dictionary for saving state across page requests.
	/// </summary>
	public interface ISessionState {
		/// <summary>Gets or sets a session value by name.</summary>
		/// <param name="key">The key name of the session value.</param>
		/// <returns>The session-state value with the specified name.  Null if the key was not found.</returns>
		/// <remarks>No exception should be thrown if the key is not found.</remarks>
		/// <exception cref="System.ArgumentNullException"><paramref name="key"/> is a null reference.</exception>
		object this[string key] { get; set; }
		/// <summary>Adds a new item to the session-state collection.</summary>
		/// <param name="key">The name of the item to add to the session-state collection.</param>
		/// <param name="value">The value of the item to add to the session-state collection.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="key"/> is a null reference.</exception>
		/// <exception cref="System.ArgumentException">An element with the same <paramref name="key"/> already exists in the dictionary.</exception>
		void Add(string key, object value);
		/// <summary>Deletes an item from the session-state collection.</summary>
		/// <param name="key">The name of the item to delete from the session-state collection.</param>
		/// <remarks>No exception should be thrown if the given <paramref name="key"/> is not in the store.</remarks>
		/// <exception cref="System.ArgumentNullException"><paramref name="key"/> is a null reference.</exception>
		void Remove(string key);
	}
}