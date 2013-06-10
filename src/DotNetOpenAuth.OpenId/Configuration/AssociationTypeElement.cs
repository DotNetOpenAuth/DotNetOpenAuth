//-----------------------------------------------------------------------
// <copyright file="AssociationTypeElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Configuration {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Describes an association type and its maximum lifetime as an element
	/// in a .config file.
	/// </summary>
	internal class AssociationTypeElement : ConfigurationElement {
		/// <summary>
		/// The name of the attribute that stores the association type.
		/// </summary>
		private const string AssociationTypeConfigName = "type";

		/// <summary>
		/// The name of the attribute that stores the association's maximum lifetime.
		/// </summary>
		private const string MaximumLifetimeConfigName = "lifetime";

		/// <summary>
		/// Initializes a new instance of the <see cref="AssociationTypeElement"/> class.
		/// </summary>
		internal AssociationTypeElement() {
		}

		/// <summary>
		/// Gets or sets the protocol name of the association.
		/// </summary>
		[ConfigurationProperty(AssociationTypeConfigName, IsRequired = true, IsKey = true)]
		////[StringValidator(MinLength = 1)]
		public string AssociationType {
			get { return (string)this[AssociationTypeConfigName]; }
			set { this[AssociationTypeConfigName] = value; }
		}

		/// <summary>
		/// Gets or sets the maximum time a shared association should live.
		/// </summary>
		/// <value>The default value is 14 days.</value>
		[ConfigurationProperty(MaximumLifetimeConfigName, IsRequired = true)]
		public TimeSpan MaximumLifetime {
			get { return (TimeSpan)this[MaximumLifetimeConfigName]; }
			set { this[MaximumLifetimeConfigName] = value; }
		}
	}
}
