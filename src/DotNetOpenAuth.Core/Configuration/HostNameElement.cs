//-----------------------------------------------------------------------
// <copyright file="HostNameElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System.Configuration;

	/// <summary>
	/// Represents the name of a single host or a regex pattern for host names.
	/// </summary>
	internal class HostNameElement : ConfigurationElement {
		/// <summary>
		/// Gets the name of the @name attribute.
		/// </summary>
		private const string NameConfigName = "name";

		/// <summary>
		/// Initializes a new instance of the <see cref="HostNameElement"/> class.
		/// </summary>
		internal HostNameElement() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HostNameElement"/> class.
		/// </summary>
		/// <param name="name">The default value of the <see cref="Name"/> property.</param>
		internal HostNameElement(string name) {
			this.Name = name;
		}

		/// <summary>
		/// Gets or sets the name of the host on the white or black list.
		/// </summary>
		[ConfigurationProperty(NameConfigName, IsRequired = true, IsKey = true)]
		////[StringValidator(MinLength = 1)]
		public string Name {
			get { return (string)this[NameConfigName]; }
			set { this[NameConfigName] = value; }
		}
	}
}
