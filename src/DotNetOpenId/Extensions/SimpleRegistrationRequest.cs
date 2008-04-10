/********************************************************
 * Copyright (C) 2007 Andrew Arnott
 * Released under the New BSD License
 * License available here: http://www.opensource.org/licenses/bsd-license.php
 * For news or support on this file: http://blog.nerdbank.net/
 ********************************************************/

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// Specifies what level of interest a relying party has in obtaining the value
	/// of a given field offered by the Simple Registration extension.
	/// </summary>
	public enum SimpleRegistrationRequest {
		/// <summary>
		/// The relying party has no interest in obtaining this field.
		/// </summary>
		NoRequest,
		/// <summary>
		/// The relying party would like the value of this field, but wants
		/// the Provider to display the field to the user as optionally provided.
		/// </summary>
		Request,
		/// <summary>
		/// The relying party considers this a required field as part of
		/// authentication.  The Provider and/or user agent MAY still choose to
		/// not provide the value of the field however, according to the
		/// Simple Registration extension specification.
		/// </summary>
		Require,
	}
}