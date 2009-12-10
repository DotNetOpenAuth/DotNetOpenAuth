//-----------------------------------------------------------------------
// <copyright file="ReportingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Represents the &lt;reporting&gt; element in the host's .config file.
	/// </summary>
	internal class ReportingElement : ConfigurationElement {
		/// <summary>
		/// The name of the @enabled attribute.
		/// </summary>
		private const string EnabledAttributeName = "enabled";

		/// <summary>
		/// Initializes a new instance of the <see cref="ReportingElement"/> class.
		/// </summary>
		internal ReportingElement() {
		}

		/// <summary>
		/// Gets or sets a value indicating whether this reporting is enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		[ConfigurationProperty(EnabledAttributeName, DefaultValue = true)]
		internal bool Enabled {
			get { return (bool)this[EnabledAttributeName]; }
			set { this[EnabledAttributeName] = value; }
		}
	}
}
