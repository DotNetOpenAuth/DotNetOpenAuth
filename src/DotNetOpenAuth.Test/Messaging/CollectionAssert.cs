//-----------------------------------------------------------------------
// <copyright file="CollectionAssert.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using NUnit.Framework;
	using Validation;

	internal class CollectionAssert<T> {
		internal static void AreEquivalent(ICollection<T> expected, ICollection<T> actual) {
			Requires.NotNull(expected, "expected");
			Requires.NotNull(actual, "actual");

			ICollection expectedNonGeneric = new List<T>(expected);
			ICollection actualNonGeneric = new List<T>(actual);
			CollectionAssert.AreEquivalent(expectedNonGeneric, actualNonGeneric);
		}

		internal static void AreEquivalentByEquality(ICollection<T> expected, ICollection<T> actual) {
			Requires.NotNull(expected, "expected");
			Requires.NotNull(actual, "actual");

			Assert.AreEqual(expected.Count, actual.Count);
			foreach (T value in expected) {
				Assert.IsTrue(actual.Contains(value));
			}
		}

		internal static void Contains(IEnumerable<T> sequence, T element) {
			Requires.NotNull(sequence, "sequence");

			if (!sequence.Contains(element)) {
				Assert.Fail("Sequence did not include expected element '{0}'.", element);
			}
		}
	}
}
