//-----------------------------------------------------------------------
// <copyright file="TypeConfigurationCollection.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A collection of <see cref="TypeConfigurationElement&lt;T&gt;"/>.
	/// </summary>
	/// <typeparam name="T">The type that all types specified in the elements must derive from.</typeparam>
	internal class TypeConfigurationCollection<T> : ConfigurationElementCollection
		where T : class {
		/// <summary>
		/// Initializes a new instance of the TypeConfigurationCollection class.
		/// </summary>
		internal TypeConfigurationCollection() {
		}

		/// <summary>
		/// Initializes a new instance of the TypeConfigurationCollection class.
		/// </summary>
		/// <param name="elements">The elements that should be added to the collection initially.</param>
		internal TypeConfigurationCollection(IEnumerable<Type> elements) {
			Requires.NotNull(elements, "elements");

			foreach (Type element in elements) {
				this.BaseAdd(new TypeConfigurationElement<T> { TypeName = element.AssemblyQualifiedName });
			}
		}

		/// <summary>
		/// Creates instances of all the types listed in the collection.
		/// </summary>
		/// <param name="allowInternals">if set to <c>true</c> then internal types may be instantiated.</param>
		/// <param name="hostFactories">The host factories.</param>
		/// <returns>
		/// A sequence of instances generated from types in this collection.  May be empty, but never null.
		/// </returns>
		internal IEnumerable<T> CreateInstances(bool allowInternals, IHostFactories hostFactories) {
			return from element in this.Cast<TypeConfigurationElement<T>>()
			       where !element.IsEmpty
			       select element.CreateInstance(default(T), allowInternals, hostFactories);
		}

		/// <summary>
		/// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"/>.
		/// </summary>
		/// <returns>
		/// A new <see cref="T:System.Configuration.ConfigurationElement"/>.
		/// </returns>
		protected override ConfigurationElement CreateNewElement() {
			return new TypeConfigurationElement<T>();
		}

		/// <summary>
		/// Gets the element key for a specified configuration element when overridden in a derived class.
		/// </summary>
		/// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
		/// </returns>
		protected override object GetElementKey(ConfigurationElement element) {
			Requires.NotNull(element, "element");
			TypeConfigurationElement<T> typedElement = (TypeConfigurationElement<T>)element;
			return (!string.IsNullOrEmpty(typedElement.TypeName) ? typedElement.TypeName : typedElement.XamlSource) ?? string.Empty;
		}
	}
}
