//-----------------------------------------------------------------------
// <copyright file="AXAttributeFormats.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;

	/// <summary>
	/// The various Type URI formats an AX attribute may use by various remote parties.
	/// </summary>
	[Flags]
	public enum AXAttributeFormats {
		/// <summary>
		/// No attribute format.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// AX attributes should use the Type URI format starting with <c>http://axschema.org/</c>.
		/// </summary>
		AXSchemaOrg = 0x1,

		/// <summary>
		/// AX attributes should use the Type URI format starting with <c>http://schema.openid.net/</c>.
		/// </summary>
		SchemaOpenIdNet = 0x2,

		/// <summary>
		/// AX attributes should use the Type URI format starting with <c>http://openid.net/schema/</c>.
		/// </summary>
		OpenIdNetSchema = 0x4,

		/// <summary>
		/// All known schemas.
		/// </summary>
		All = 0xff,

		/// <summary>
		/// The most common schemas.
		/// </summary>
		Common = AXSchemaOrg | SchemaOpenIdNet,
	}
}
