//-----------------------------------------------------------------------
// <copyright file="ClaimTypeUriConverter.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ComponentModel {
	using System;
	using System.IdentityModel.Claims;

	/// <summary>
	/// A design-time helper to give a Uri property an auto-complete functionality
	/// listing the URIs in the <see cref="ClaimTypes"/> class.
	/// </summary>
	public class ClaimTypeUriConverter : UriConverter {
		/// <summary>
		/// Initializes a new instance of the <see cref="ClaimTypeUriConverter"/> class.
		/// </summary>
		[Obsolete("This class is meant for design-time use within an IDE, and not meant to be used directly by runtime code.")]
		public ClaimTypeUriConverter() {
		}

		/// <summary>
		/// Gets the type to reflect over to extract the well known values.
		/// </summary>
		protected override Type WellKnownValuesType {
			get { return typeof(ClaimTypes); }
		}
	}
}
