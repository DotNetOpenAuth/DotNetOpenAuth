//-----------------------------------------------------------------------
// <copyright file="EmptyEnumerator.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.Collections;

	/// <summary>
	/// An enumerator that always generates zero elements.
	/// </summary>
	internal class EmptyEnumerator : IEnumerator {
		/// <summary>
		/// The singleton instance of this empty enumerator.
		/// </summary>
		internal static readonly EmptyEnumerator Instance = new EmptyEnumerator();

		/// <summary>
		/// Prevents a default instance of the <see cref="EmptyEnumerator"/> class from being created.
		/// </summary>
		private EmptyEnumerator() {
		}

		#region IEnumerator Members

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The current element in the collection.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		/// The enumerator is positioned before the first element of the collection or after the last element.
		/// </exception>
		public object Current {
			get { return null; }
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		/// The collection was modified after the enumerator was created.
		/// </exception>
		public bool MoveNext() {
			return false;
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">
		/// The collection was modified after the enumerator was created.
		/// </exception>
		public void Reset() {
		}

		#endregion
	}
}
