//-----------------------------------------------------------------------
// <copyright file="EnumerableCacheExtensions.cs" company="Outercurve Foundation">
// Copyright (c) Outercurve Foundation. All rights reserved.
// This code is released under the Microsoft Public License (Ms-PL).
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Validation;

	/// <summary>
	/// Extension methods for <see cref="IEnumerable&lt;T&gt;"/> types.
	/// </summary>
	public static class EnumerableCacheExtensions {
		/// <summary>
		/// Caches the results of enumerating over a given object so that subsequence enumerations
		/// don't require interacting with the object a second time.
		/// </summary>
		/// <typeparam name="T">The type of element found in the enumeration.</typeparam>
		/// <param name="sequence">The enumerable object.</param>
		/// <returns>
		/// Either a new enumerable object that caches enumerated results, or the original, <paramref name="sequence"/>
		/// object if no caching is necessary to avoid additional CPU work.
		/// </returns>
		/// <remarks>
		///   <para>This is designed for use on the results of generator methods (the ones with <c>yield return</c> in them)
		/// so that only those elements in the sequence that are needed are ever generated, while not requiring
		/// regeneration of elements that are enumerated over multiple times.</para>
		///   <para>This can be a huge performance gain if enumerating multiple times over an expensive generator method.</para>
		///   <para>Some enumerable types such as collections, lists, and already-cached generators do not require
		/// any (additional) caching, and this method will simply return those objects rather than caching them
		/// to avoid double-caching.</para>
		/// </remarks>
		public static IEnumerable<T> CacheGeneratedResults<T>(this IEnumerable<T> sequence) {
			Requires.NotNull(sequence, "sequence");

			// Don't create a cache for types that don't need it.
			if (sequence is IList<T> ||
			  sequence is ICollection<T> ||
			  sequence is Array ||
			  sequence is EnumerableCache<T>) {
				return sequence;
			}

			return new EnumerableCache<T>(sequence);
		}

		/// <summary>
		/// A wrapper for <see cref="IEnumerable&lt;T&gt;"/> types and returns a caching <see cref="IEnumerator&lt;T&gt;"/>
		/// from its <see cref="IEnumerable&lt;T&gt;.GetEnumerator"/> method.
		/// </summary>
		/// <typeparam name="T">The type of element in the sequence.</typeparam>
		private class EnumerableCache<T> : IEnumerable<T> {
			/// <summary>
			/// The results from enumeration of the live object that have been collected thus far.
			/// </summary>
			private List<T> cache;

			/// <summary>
			/// The original generator method or other enumerable object whose contents should only be enumerated once.
			/// </summary>
			private IEnumerable<T> generator;

			/// <summary>
			/// The enumerator we're using over the generator method's results.
			/// </summary>
			private IEnumerator<T> generatorEnumerator;

			/// <summary>
			/// The sync object our caching enumerators use when adding a new live generator method result to the cache.
			/// </summary>
			/// <remarks>
			/// Although individual enumerators are not thread-safe, this <see cref="IEnumerable&lt;T&gt;"/> should be
			/// thread safe so that multiple enumerators can be created from it and used from different threads.
			/// </remarks>
			private object generatorLock = new object();

			/// <summary>
			/// Initializes a new instance of the EnumerableCache class.
			/// </summary>
			/// <param name="generator">The generator.</param>
			internal EnumerableCache(IEnumerable<T> generator) {
				Requires.NotNull(generator, "generator");

				this.generator = generator;
			}

			#region IEnumerable<T> Members

			/// <summary>
			/// Returns an enumerator that iterates through the collection.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
			/// </returns>
			public IEnumerator<T> GetEnumerator() {
				if (this.generatorEnumerator == null) {
					this.cache = new List<T>();
					this.generatorEnumerator = this.generator.GetEnumerator();
				}

				return new EnumeratorCache(this);
			}

			#endregion

			#region IEnumerable Members

			/// <summary>
			/// Returns an enumerator that iterates through a collection.
			/// </summary>
			/// <returns>
			/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
			/// </returns>
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
				return this.GetEnumerator();
			}

			#endregion

			/// <summary>
			/// An enumerator that uses cached enumeration results whenever they are available,
			/// and caches whatever results it has to pull from the original <see cref="IEnumerable&lt;T&gt;"/> object.
			/// </summary>
			private class EnumeratorCache : IEnumerator<T> {
				/// <summary>
				/// The parent enumeration wrapper class that stores the cached results.
				/// </summary>
				private EnumerableCache<T> parent;

				/// <summary>
				/// The position of this enumerator in the cached list.
				/// </summary>
				private int cachePosition = -1;

				/// <summary>
				/// Initializes a new instance of the EnumeratorCache class.
				/// </summary>
				/// <param name="parent">The parent cached enumerable whose GetEnumerator method is calling this constructor.</param>
				internal EnumeratorCache(EnumerableCache<T> parent) {
					Requires.NotNull(parent, "parent");

					this.parent = parent;
				}

				#region IEnumerator<T> Members

				/// <summary>
				/// Gets the element in the collection at the current position of the enumerator.
				/// </summary>
				/// <returns>
				/// The element in the collection at the current position of the enumerator.
				/// </returns>
				public T Current {
					get {
						if (this.cachePosition < 0 || this.cachePosition >= this.parent.cache.Count) {
							throw new InvalidOperationException();
						}

						return this.parent.cache[this.cachePosition];
					}
				}

				#endregion

				#region IEnumerator Properties

				/// <summary>
				/// Gets the element in the collection at the current position of the enumerator.
				/// </summary>
				/// <returns>
				/// The element in the collection at the current position of the enumerator.
				/// </returns>
				object System.Collections.IEnumerator.Current {
					get { return this.Current; }
				}

				#endregion

				#region IDisposable Members

				/// <summary>
				/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
				/// </summary>
				public void Dispose() {
					this.Dispose(true);
					GC.SuppressFinalize(this);
				}

				#endregion

				#region IEnumerator Methods

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
					this.cachePosition++;
					if (this.cachePosition >= this.parent.cache.Count) {
						lock (this.parent.generatorLock) {
							if (this.parent.generatorEnumerator.MoveNext()) {
								this.parent.cache.Add(this.parent.generatorEnumerator.Current);
							} else {
								return false;
							}
						}
					}

					return true;
				}

				/// <summary>
				/// Sets the enumerator to its initial position, which is before the first element in the collection.
				/// </summary>
				/// <exception cref="T:System.InvalidOperationException">
				/// The collection was modified after the enumerator was created.
				/// </exception>
				public void Reset() {
					this.cachePosition = -1;
				}

				#endregion

				/// <summary>
				/// Releases unmanaged and - optionally - managed resources
				/// </summary>
				/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
				protected virtual void Dispose(bool disposing) {
					// Nothing to do here.
				}
			}
		}
	}
}
