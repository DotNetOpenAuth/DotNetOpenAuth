//-----------------------------------------------------------------------
// <copyright file="AXFetchAsSregTransform.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Behaviors {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

	/// <summary>
	/// An Attribute Exchange and Simple Registration filter to make all incoming attribute 
	/// requests look like Simple Registration requests, and to convert the response
	/// to the originally requested extension and format.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sreg", Justification = "Abbreviation")]
	public class AXFetchAsSregTransform {
		/// <summary>
		/// Initializes static members of the <see cref="AXFetchAsSregTransform"/> class.
		/// </summary>
		static AXFetchAsSregTransform() {
			AXFormats = AXAttributeFormats.Common;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AXFetchAsSregTransform"/> class.
		/// </summary>
		public AXFetchAsSregTransform() {
		}

		/// <summary>
		/// Gets or sets the AX attribute type URI formats this transform is willing to work with.
		/// </summary>
		public static AXAttributeFormats AXFormats { get; set; }
	}
}
