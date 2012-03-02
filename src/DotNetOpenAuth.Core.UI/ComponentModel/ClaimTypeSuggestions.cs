//-----------------------------------------------------------------------
// <copyright file="ClaimTypeSuggestions.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ComponentModel {
	using System;
	using System.Diagnostics.Contracts;
	using System.IdentityModel.Claims;

	/// <summary>
	/// A design-time helper to give a Uri property an auto-complete functionality
	/// listing the URIs in the <see cref="ClaimTypes"/> class.
	/// </summary>
	public class ClaimTypeSuggestions : SuggestedStringsConverter {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClaimTypeSuggestions"/> class.
		/// </summary>
		[Obsolete("This class is meant for design-time use within an IDE, and not meant to be used directly by runtime code.")]
		public ClaimTypeSuggestions() {
		}

		/// <summary>
		/// Gets the type to reflect over to extract the well known values.
		/// </summary>
		[Pure]
		protected override Type WellKnownValuesType {
			get { return typeof(ClaimTypes); }
		}
	}
}
