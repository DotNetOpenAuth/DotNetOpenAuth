//-----------------------------------------------------------------------
// <copyright file="IssuersUriConverter.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ComponentModel {
	using System;
	using DotNetOpenAuth.InfoCard;

	/// <summary>
	/// A design-time helper to give a Uri property an auto-complete functionality
	/// listing the URIs in the <see cref="WellKnownIssuers"/> class.
	/// </summary>
	public class IssuersUriConverter : UriConverter {
		/// <summary>
		/// Initializes a new instance of the <see cref="IssuersUriConverter"/> class.
		/// </summary>
		[Obsolete("This class is meant for design-time use within an IDE, and not meant to be used directly by runtime code.")]
		public IssuersUriConverter() {
		}

		/// <summary>
		/// Gets the type to reflect over to extract the well known values.
		/// </summary>
		protected override Type WellKnownValuesType {
			get { return typeof(WellKnownIssuers); }
		}
	}
}
