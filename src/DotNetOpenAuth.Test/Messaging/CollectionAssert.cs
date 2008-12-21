//-----------------------------------------------------------------------
// <copyright file="CollectionAssert.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System.Collections;
	using System.Collections.Generic;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	internal class CollectionAssert<T> {
		internal static void AreEquivalent(ICollection<T> expected, ICollection<T> actual) {
			ICollection expectedNonGeneric = new List<T>(expected);
			ICollection actualNonGeneric = new List<T>(actual);
			CollectionAssert.AreEquivalent(expectedNonGeneric, actualNonGeneric);
		}

		internal static void AreEquivalentByEquality(ICollection<T> expected, ICollection<T> actual) {
			Assert.AreEqual(expected.Count, actual.Count);
			foreach (T value in expected) {
				Assert.IsTrue(actual.Contains(value));
			}
		}
	}
}
