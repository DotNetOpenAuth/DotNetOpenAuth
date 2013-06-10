//-----------------------------------------------------------------------
// <copyright file="AssociationTypeCollection.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Collections.Generic;
	using System.Configuration;

	/// <summary>
	/// Describes a collection of association type sub-elements in a .config file.
	/// </summary>
	internal class AssociationTypeCollection : ConfigurationElementCollection, IEnumerable<AssociationTypeElement> {
		/// <summary>
		/// Initializes a new instance of the <see cref="AssociationTypeCollection"/> class.
		/// </summary>
		public AssociationTypeCollection() {
		}

		#region IEnumerable<AssociationTypeElement> Members

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public new IEnumerator<AssociationTypeElement> GetEnumerator() {
			for (int i = 0; i < Count; i++) {
				yield return (AssociationTypeElement)BaseGet(i);
			}
		}

		#endregion

		/// <summary>
		/// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"/>.
		/// </summary>
		/// <returns>
		/// A new <see cref="T:System.Configuration.ConfigurationElement"/>.
		/// </returns>
		protected override ConfigurationElement CreateNewElement() {
			return new AssociationTypeElement();
		}

		/// <summary>
		/// Gets the element key for a specified configuration element when overridden in a derived class.
		/// </summary>
		/// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
		/// </returns>
		protected override object GetElementKey(ConfigurationElement element) {
			return ((AssociationTypeElement)element).AssociationType ?? string.Empty;
		}
	}
}
