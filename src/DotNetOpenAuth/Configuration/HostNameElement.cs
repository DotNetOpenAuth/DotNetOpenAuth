//-----------------------------------------------------------------------
// <copyright file="HostNameElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
