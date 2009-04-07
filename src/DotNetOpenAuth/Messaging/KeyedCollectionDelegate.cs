//-----------------------------------------------------------------------
// <copyright file="KeyedCollectionDelegate.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.ObjectModel;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// A KeyedCollection whose item -&gt; key transform is provided via a delegate
	/// to its constructor, and null items are disallowed.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TItem">The type of the item.</typeparam>
	[Serializable]
	internal class KeyedCollectionDelegate<TKey, TItem> : KeyedCollection<TKey, TItem> {
		/// <summary>
		/// The delegate that returns a key for the given item.
		/// </summary>
		private Func<TItem, TKey> getKeyForItemDelegate;

		/// <summary>
		/// Initializes a new instance of the KeyedCollectionDelegate class.
		/// </summary>
		/// <param name="getKeyForItemDelegate">The delegate that gets the key for a given item.</param>
		internal KeyedCollectionDelegate(Func<TItem, TKey> getKeyForItemDelegate) {
			Contract.Requires(getKeyForItemDelegate != null);
			ErrorUtilities.VerifyArgumentNotNull(getKeyForItemDelegate, "getKeyForItemDelegate");

			this.getKeyForItemDelegate = getKeyForItemDelegate;
		}

		/// <summary>
		/// When implemented in a derived class, extracts the key from the specified element.
		/// </summary>
		/// <param name="item">The element from which to extract the key.</param>
		/// <returns>The key for the specified element.</returns>
		protected override TKey GetKeyForItem(TItem item) {
			ErrorUtilities.VerifyArgumentNotNull(item, "item");
			return this.getKeyForItemDelegate(item);
		}
	}
}
