//-----------------------------------------------------------------------
// <copyright file="CollectionAssert.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using NUnit.Framework;

	internal class CollectionAssert<T> {
		internal static void AreEquivalent(ICollection<T> expected, ICollection<T> actual) {
			Contract.Requires<ArgumentNullException>(expected != null);
			Contract.Requires<ArgumentNullException>(actual != null);

			ICollection expectedNonGeneric = new List<T>(expected);
			ICollection actualNonGeneric = new List<T>(actual);
			CollectionAssert.AreEquivalent(expectedNonGeneric, actualNonGeneric);
		}

		internal static void AreEquivalentByEquality(ICollection<T> expected, ICollection<T> actual) {
			Contract.Requires<ArgumentNullException>(expected != null);
			Contract.Requires<ArgumentNullException>(actual != null);

			Assert.AreEqual(expected.Count, actual.Count);
			foreach (T value in expected) {
				Assert.IsTrue(actual.Contains(value));
			}
		}

		internal static void Contains(IEnumerable<T> sequence, T element) {
			Contract.Requires<ArgumentNullException>(sequence != null);

			if (!sequence.Contains(element)) {
				Assert.Fail("Sequence did not include expected element '{0}'.", element);
			}
		}
	}
}
