//-----------------------------------------------------------------------
// <copyright file="EnumerableCacheTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
//     This code is released under the Microsoft Public License (Ms-PL).
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Tests for cached enumeration.
	/// </summary>
	[TestClass]
	public class EnumerableCacheTests {
		/// <summary>
		/// The number of times the generator method's implementation is started.
		/// </summary>
		private int generatorInvocations;

		/// <summary>
		/// The number of times the end of the generator method's implementation is reached.
		/// </summary>
		private int generatorCompleted;

		/// <summary>
		/// Gets or sets the test context.
		/// </summary>
		public TestContext TestContext { get; set; }

		/// <summary>
		/// Sets up a test.
		/// </summary>
		[TestInitialize]
		public void Setup() {
			this.generatorInvocations = 0;
			this.generatorCompleted = 0;
		}

		[TestMethod]
		public void EnumerableCache() {
			// Baseline
			var generator = this.NumberGenerator();
			var list1 = generator.ToList();
			var list2 = generator.ToList();
			Assert.AreEqual(2, this.generatorInvocations);
			CollectionAssert.AreEqual(list1, list2);

			// Cache behavior
			this.generatorInvocations = 0;
			this.generatorCompleted = 0;
			generator = this.NumberGenerator().CacheGeneratedResults();
			var list3 = generator.ToList();
			var list4 = generator.ToList();
			Assert.AreEqual(1, this.generatorInvocations);
			Assert.AreEqual(1, this.generatorCompleted);
			CollectionAssert.AreEqual(list1, list3);
			CollectionAssert.AreEqual(list1, list4);
		}

		[TestMethod]
		public void GeneratesOnlyRequiredElements() {
			var generator = this.NumberGenerator().CacheGeneratedResults();
			Assert.AreEqual(0, this.generatorInvocations);
			generator.Take(2).ToList();
			Assert.AreEqual(1, this.generatorInvocations);
			Assert.AreEqual(0, this.generatorCompleted, "Only taking part of the list should not have completed the generator.");
		}

		[TestMethod]
		public void PassThruDoubleCache() {
			var cache1 = this.NumberGenerator().CacheGeneratedResults();
			var cache2 = cache1.CacheGeneratedResults();
			Assert.AreSame(cache1, cache2, "Two caches were set up rather than just sharing the first one.");
		}

		[TestMethod]
		public void PassThruList() {
			var list = this.NumberGenerator().ToList();
			var cache = list.CacheGeneratedResults();
			Assert.AreSame(list, cache);
		}

		[TestMethod]
		public void PassThruArray() {
			var array = this.NumberGenerator().ToArray();
			var cache = array.CacheGeneratedResults();
			Assert.AreSame(array, cache);
		}

		[TestMethod]
		public void PassThruCollection() {
			var collection = new Collection<int>();
			var cache = collection.CacheGeneratedResults();
			Assert.AreSame(collection, cache);
		}

		/// <summary>
		/// Tests calling IEnumerator.Current before first call to MoveNext.
		/// </summary>
		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void EnumerableCacheCurrentThrowsBefore() {
			var foo = this.NumberGenerator().CacheGeneratedResults().GetEnumerator().Current;
		}

		/// <summary>
		/// Tests calling IEnumerator.Current after MoveNext returns false.
		/// </summary>
		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void EnumerableCacheCurrentThrowsAfter() {
			var enumerator = this.NumberGenerator().CacheGeneratedResults().GetEnumerator();
			while (enumerator.MoveNext()) {
			}
			var foo = enumerator.Current;
		}

		private IEnumerable<int> NumberGenerator() {
			this.generatorInvocations++;
			for (int i = 10; i < 15; i++) {
				yield return i;
			}
			this.generatorCompleted++;
		}
	}
}
