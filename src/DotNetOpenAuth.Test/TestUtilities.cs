//-----------------------------------------------------------------------
// <copyright file="TestUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// An assortment of methods useful for testing.
	/// </summary>
	internal class TestUtilities {
		/// <summary>
		/// Tests whether two arrays are equal in length and contents.
		/// </summary>
		/// <typeparam name="T">The type of elements in the arrays.</typeparam>
		/// <param name="first">The first array to test.  May not be null.</param>
		/// <param name="second">The second array to test. May not be null.</param>
		/// <returns>True if the arrays equal; false otherwise.</returns>
		public static bool AreEquivalent<T>(T[] first, T[] second) {
			if (first == null) {
				throw new ArgumentNullException("first");
			}
			if (second == null) {
				throw new ArgumentNullException("second");
			}
			if (first.Length != second.Length) {
				return false;
			}
			for (int i = 0; i < first.Length; i++) {
				if (!first[i].Equals(second[i])) {
					return false;
				}
			}
			return true;
		}

		public static bool AreEquivalent<TKey, TValue>(IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second) {
			return AreEquivalent(first.ToArray(), second.ToArray());
		}
	}
}
