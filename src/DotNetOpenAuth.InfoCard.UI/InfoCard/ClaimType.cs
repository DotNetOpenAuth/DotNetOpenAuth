//-----------------------------------------------------------------------
// <copyright file="ClaimType.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.InfoCard {
	using System;
	using System.ComponentModel;
	using System.IdentityModel.Claims;
	using System.Web.UI;

	/// <summary>
	/// Description of a claim that is requested or required in a submitted Information Card.
	/// </summary>
	[PersistChildren(false)]
	[Serializable]
	public class ClaimType {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClaimType"/> class.
		/// </summary>
		public ClaimType() {
		}

		/// <summary>
		/// Gets or sets the URI of a requested claim.
		/// </summary>
		/// <remarks>
		/// For a list of well-known claim type URIs, see the <see cref="ClaimTypes"/> class.
		/// </remarks>
		[TypeConverter(typeof(ComponentModel.ClaimTypeSuggestions))]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this claim is optional.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is optional; otherwise, <c>false</c>.
		/// </value>
		[DefaultValue(false)]
		public bool IsOptional { get; set; }

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString() {
			return this.Name ?? "<no name>";
		}
	}
}
